﻿using System.Threading.Tasks;
using SkyChain.Web;
using static Revital.User;

namespace Revital
{
    public abstract class BuyWork : WebWork
    {
    }

    public class MyBuyWork : BuyWork
    {
        protected override void OnMake()
        {
            MakeVarWork<MyBuyVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE uid = @1 AND status > 0  ORDER BY id DESC LIMIT 5 OFFSET 5 * @2");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(prin.id).Set(page));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.pay, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }

    [UserAuthorize(orgly: ORGLY_OP)]
    public abstract class BizlyBuyWork : BuyWork
    {
        protected override void OnMake()
        {
            MakeVarWork<BizlyBuyVarWork>();
        }

        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE bizid = @1 AND status > 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(org.id));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.pay, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }

        public async Task closed(WebContext wc)
        {
            short orgid = wc[-1];
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Buy.Empty).T(" FROM buys WHERE orgid = @1 AND status >= ORDER BY id DESC");
            var arr = await dc.QueryAsync<Buy>(p => p.Set(orgid));
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                h.TABLE(arr, o =>
                {
                    h.TD_().A_TEL(o.uname, o.utel)._TD();
                    h.TD(o.pay, true);
                    // h.TD(Statuses[o.status]);
                });
            });
        }
    }

    [Ui("客户订单管理", forkie: Item.TYP_AGRI)]
    public class AgriBizlyBuyWork : BizlyBuyWork
    {
    }

    [Ui("客户预订管理", forkie: Item.TYP_DIETARY)]
    public class DietaryBizlyBuyWork : BizlyBuyWork
    {
    }
}