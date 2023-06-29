using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainSmart.WeixinUtility;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public class MgtService : MainService
{
    protected override void OnCreate()
    {
        CreateWork<AdmlyWork>("admly"); // for admin

        CreateWork<SuplyWork>("suply"); // for centers and supply shops

        CreateWork<RtllyWork>("rtlly"); // for markets and retail shops
    }

    public void @default(WebContext wc)
    {
        wc.GivePage(200, h =>
        {
            h.FORM_().FIELDSUL_("运营管理模块");
            h.LI_().A_("admly/").T("Ａ）平台管理")._A()._LI();
            h.LI_().A_("suply//").T("Ｂ）供应操作")._A()._LI();
            h.LI_().A_("rtlly//").T("Ｃ）市场操作")._A()._LI();
            h._FIELDSUL()._FORM();
        }, true, 3600, title: "中惠农通运营管理");
    }

    /**
     * The callback by the payment gateway.
     */
    public async Task onpay(WebContext wc)
    {
        var xe = await wc.ReadAsync<XElem>();

        if (!OnNotified(sup: true, xe, out var trade_no, out var cash))
        {
            wc.Give(400);
            return;
        }

        int pos = 0;
        var purid = trade_no.ParseInt(ref pos);

        try
        {
            // NOTE: WCPay may send notification more than once
            using var dc = NewDbContext();

            if (await dc.QueryTopAsync("SELECT supid, hubid, lotid, qty, topay FROM purs WHERE id = @1 AND status = -1", p => p.Set(purid)))
            {
                dc.Let(out int supid);
                dc.Let(out int hubid);
                dc.Let(out int lotid);
                dc.Let(out short qty);
                dc.Let(out decimal topay);

                if (topay == cash) // verify that the ammount is correct
                {
                    // the order and the lot updates
                    dc.Sql("UPDATE purs SET status = 1, created = @1, pay = @2 WHERE id = @3 AND status = -1; UPDATE lotinvs SET stock = stock - @4 WHERE lotid = @5 AND hubid = @6");
                    await dc.ExecuteAsync(p => p.Set(DateTime.Now).Set(cash).Set(purid).Set(qty).Set(lotid).Set(hubid));

                    // put a notice to the accepter
                    var sup = GrabTwin<int, Org>(supid);
                    sup.Notices.Put(OrgNoticePack.PUR_CREATED, 1, cash);
                }
                else // the pay differs from the order
                {
                    // refund
                    await PostRefundAsync(sup: false, trade_no, cash, cash, trade_no, "支付金额与订单不符");
                }
            }
        }
        finally
        {
            // return xml to WCPay server
            var x = new XmlBuilder(true, 1024);
            x.ELEM("xml", null, () =>
            {
                x.ELEM("return_code", "SUCCESS");
                x.ELEM("return_msg", "OK");
            });
            wc.Give(200, x);
        }
    }
}