﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Entity;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.Modal;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart;

public abstract class MsgWork<V> : WebWork where V : MsgVarWork, new()
{
    protected override void OnCreate()
    {
        CreateVarWork<V>();
    }

    protected static void MainGrid(HtmlBuilder h, IList<Msg> arr)
    {
        h.MAINGRID(arr, o =>
        {
            h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");
            h.PIC("/void.webp", css: "uk-width-1-5");

            h.ASIDE_();
            h.HEADER_().H4(o.name);

            h.SPAN(Msg.Statuses[o.status], "uk-badge");
            h._HEADER();

            h.Q(o.tip, "uk-width-expand");
            h._ASIDE();

            h._A();
        });
    }
}

[Ui("消息")]
public class MktlyMsgWork : MsgWork<MktlyMsgVarWork>
{
    [Ui("消息发布", status: 1), Tool(Anchor)]
    public async Task @default(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Msg.Empty).T(" FROM msgs WHERE orgid = @1 AND status > 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Msg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无消息发布");
                return;
            }

            MainGrid(h, arr);

            h.PAGINATION(arr?.Length == 20);
        }, false, 6);
    }

    [Ui(tip: "已作废消息", icon: "trash", status: 2), Tool(Anchor)]
    public async Task @void(WebContext wc, int page)
    {
        var org = wc[-1].As<Org>();

        using var dc = NewDbContext();
        dc.Sql("SELECT ").collst(Msg.Empty).T(" FROM msgs WHERE orgid = @1 AND status = 0 ORDER BY oked DESC limit 20 OFFSET @2 * 20");
        var arr = await dc.QueryAsync<Msg>(p => p.Set(org.id).Set(page));

        wc.GivePage(200, h =>
        {
            h.TOOLBAR();

            if (arr == null)
            {
                h.ALERT("尚无已作废消息");
                return;
            }

            MainGrid(h, arr);
            h.PAGINATION(arr?.Length == 20);
        }, false, 6);
    }

    [OrglyAuthorize(0, User.ROL_MGT)]
    [Ui("新建", "新建指定类型的消息", icon: "plus", status: 7), Tool(ButtonOpen)]
    public async Task @new(WebContext wc)
    {
        var org = wc[-1].As<Org>();
        var prin = (User)wc.Principal;

        var o = new Msg
        {
            orgid = org.id,
            created = DateTime.Now,
            creator = prin.name,
        };
        if (wc.IsGet)
        {
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_(wc.Action.Tip);

                h.LI_().SELECT("消息类型", nameof(o.typ), o.typ, Msg.Typs)._LI();
                h.LI_().TEXT("标题", nameof(o.name), o.name, max: 12)._LI();
                h.LI_().TEXTAREA("内容", nameof(o.content), o.content, max: 300)._LI();
                h.LI_().TEXTAREA("注解", nameof(o.tip), o.tip, max: 40)._LI();
                h.LI_().SELECT("级别", nameof(o.rank), o.rank, Msg.Ranks)._LI();

                h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
            });
        }
        else // POST
        {
            const short msk = MSK_BORN | MSK_EDIT;
            // populate 
            var m = await wc.ReadObjectAsync(msk, o);

            // insert
            using var dc = NewDbContext();
            dc.Sql("INSERT INTO facts ").colset(Msg.Empty, msk)._VALUES_(Msg.Empty, msk);
            await dc.ExecuteAsync(p => m.Write(p, msk));

            wc.GivePane(200); // close dialog
        }
    }
}

[Ui("事项")]
public class CtrlyMsgWork : MsgWork<CtrlyMsgVarWork>
{
}