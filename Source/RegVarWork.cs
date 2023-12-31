﻿using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class RegVarWork : WebWork
{
}

public class AdmlyRegVarWork : RegVarWork
{
    public async Task @default(WebContext wc, int typ)
    {
        short id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE id = @1");
            var o = await dc.QueryTopAsync<Reg>(p => p.Set(id));
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("区域属性");
                h.LI_().NUMBER("区域编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                h.LI_().NUMBER("品类标志", nameof(o.catmsk), o.catmsk, min: 0, max: 0xff)._LI();
                h._FIELDSUL()._FORM();

                h.TOOLBAR(bottom: true);
            });
        }
        else
        {
            var o = await wc.ReadObjectAsync<Reg>();
            using var dc = NewDbContext();
            dc.Sql("UPDATE regs")._SET_(o).T(" WHERE id = @1");
            await dc.ExecuteAsync(p =>
            {
                o.Write(p);
                p.Set(id);
            });
            wc.GivePane(200);
        }
    }

    [Ui(tip: "调整区域信息", icon: "edit"), Tool(Anchor)]
    public async Task edit(WebContext wc)
    {
        short id = wc[0];
        if (wc.IsGet)
        {
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Reg.Empty).T(" FROM regs WHERE id = @1");
            var o = await dc.QueryTopAsync<Reg>(p => p.Set(id));
            wc.GivePane(200, h =>
            {
                h.FORM_().FIELDSUL_("区域属性");
                h.LI_().NUMBER("区域编号", nameof(o.id), o.id, min: 1, max: 99, required: true)._LI();
                h.LI_().TEXT("名称", nameof(o.name), o.name, min: 2, max: 10, required: true)._LI();
                h.LI_().NUMBER("排序", nameof(o.idx), o.idx, min: 1, max: 99)._LI();
                h.LI_().NUMBER("品类标志", nameof(o.catmsk), o.catmsk, min: 0, max: 0xff)._LI();
                h._FIELDSUL()._FORM();
            });
        }
        else
        {
            var o = await wc.ReadObjectAsync<Reg>();
            using var dc = NewDbContext();
            dc.Sql("UPDATE regs")._SET_(o).T(" WHERE id = @1");
            await dc.ExecuteAsync(p =>
            {
                o.Write(p);
                p.Set(id);
            });
            wc.GivePane(200);
        }
    }

    [Ui(tip: "删除", icon: "trash"), Tool(ButtonOpen)]
    public async Task rm(WebContext wc)
    {
        short id = wc[0];
        if (wc.IsGet)
        {
            const bool ok = true;
            wc.GivePane(200, h =>
            {
                h.ALERT("确定删除此项？");
                h.FORM_().HIDDEN(nameof(ok), ok)._FORM();
            });
        }
        else
        {
            using var dc = NewDbContext();
            dc.Sql("DELETE FROM regs WHERE id = @1");
            await dc.ExecuteAsync(p => p.Set(id));

            wc.GivePane(200);
        }
    }
}