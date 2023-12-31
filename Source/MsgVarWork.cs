﻿using System;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;

namespace ChainSmart;

public abstract class MsgVarWork : WebWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();

        const short msk = 255 | MSK_AUX;

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Msg.Empty, msk).T(" FROM msgs WHERE id = @1 AND orgid = @2");
        var o = await dc.QueryTopAsync<Msg>(p => p.Set(id).Set(org.id), msk);

        wc.GivePane(200, h =>
        {
            h.UL_("uk-list uk-list-divider");

            h.LI_().FIELD("消息标题", o.name)._LI();
            h.LI_().FIELD("内容", o.content)._LI();
            h.LI_().FIELD("注解", string.IsNullOrEmpty(o.tip) ? "无" : o.tip)._LI();
            h.LI_().FIELD("状态", o.status, Msg.Statuses).FIELD2("创建", o.creator, o.created, sep: "<br>")._LI();
            h.LI_().FIELD2("调整", o.adapter, o.adapted, sep: "<br>").FIELD2(o.IsVoid ? "作废" : "发布", o.oker, o.oked, sep: "<br>")._LI();

            h._UL();

            h.TOOLBAR(bottom: true, status: o.Status, state: o.ToState());
        }, false, 6);
    }
}

public class MktlyMsgVarWork : MsgVarWork
{
    [OrglyAuthorize(0, User.ROL_OPN)]
    [Ui(tip: "修改或调整消息", icon: "pencil", status: 1 | 2 | 4), Tool(ButtonShow)]
    public async Task edit(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Msg.Empty).T(" FROM msgs WHERE id = @1");
            var o = await dc.QueryTopAsync<Msg>(p => p.Set(id));

            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("消息类型", nameof(o.typ), o.typ, Msg.Typs)._LI();
                h.LI_().TEXT("标题", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("内容", nameof(o.content), o.content, max: 300)._LI();
                h.LI_().TEXTAREA("注解", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("级别", nameof(o.rank), o.rank, Msg.Ranks)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(edit))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_EDIT;
            var m = await wc.ReadObjectAsync(msk, new Msg
            {
                adapted = DateTime.Now,
                adapter = prin.name,
            });

            // update
            using var dc = NewDbContext();
            dc.Sql("UPDATE msgs ")._SET_(Msg.Empty, msk).T(" WHERE id = @1 AND orgid = @2");
            await dc.ExecuteAsync(p =>
            {
                m.Write(p, msk);
                p.Set(id).Set(org.id);
            });

            wc.GivePane(200); // close dialog
        }
    }


    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("发布", "安排发布", status: 1 | 2 | 4), Tool(ButtonConfirm)]
    public async Task ok(WebContext wc)
    {
        int id = wc[0];
        var org = wc[-2].As<Org>();
        var prin = (User)wc.Principal;

        using var dc = NewDbContext();
        dc.Sql("UPDATE msgs SET status = 4, oked = @1, oker = @2 WHERE id = @3 AND orgid = @4 RETURNING ").collst(Msg.Empty);
        var o = await dc.QueryTopAsync<Msg>(p => p.Set(DateTime.Now).Set(prin.name).Set(id).Set(org.id));

        org.EventPack.AddMsg(o);

        wc.GivePane(200);
    }
}

public class CtrlyMsgVarWork : MsgVarWork
{
}