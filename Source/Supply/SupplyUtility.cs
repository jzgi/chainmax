﻿using System;
using System.Text;
using System.Threading.Tasks;
using SkyChain;
using SkyChain.Db;
using SkyChain.Web;
using static Zhnt._Doc;

namespace Zhnt.Supply
{
    public static class SupplyUtility
    {
        public const string LOTS = "健康拼团";

        public static short[] GetRelatedOrgs(this Map<short, Org> orgs, Reg reg)
        {
            var lst = new ValueList<short>(32);
            for (int i = 0; i < orgs.Count; i++)
            {
                var org = orgs.ValueAt(i);
                if (org.IsMerchantTo(reg) || org.IsSocialTo(reg))
                {
                    lst.Add(org.id);
                }
            }
            return lst.ToArray();
        }

        public static void ViewLotList(this HtmlContent h, Purchase[] arr, Map<short, Org> orgs = null, DateTime today = default)
        {
            var wrk = h.Web.Work;
            h.MAIN_("uk-board");
            for (int i = 0; i < arr?.Length; i++)
            {
                var m = arr[i];
                var org = orgs[m.orgid];
                var descr = LotDescr.All[m.typ];

                h.FORM_("uk-card uk-card-default");
                h.HEADER_("uk-card-header").T(m.name);
                h.SPAN_("uk-badge uk-badge-secondary").T(m.ended, 3, 0);
                if (wrk is OrglyLotWork)
                {
                    h.SP().PICK(m.id, toolbar: true);
                }
                h._SPAN();
                h._HEADER();

                h.ARTICLE_("uk-card-body uk-flex uk-background-muted");
                h.PIC_("uk-width-1-5 uk-margin-auto-vertical uk-margin-small-right", circle: true);
                if (m.HasIcon)
                {
                    h.T(m.id).T("/icon");
                }
                else // symbolic icon
                {
                    h.T("/lot-").T(m.typ).T(".png");
                }
                h._PIC();
                h.SECTION_("uk-width-expand uk-col");
                descr.CardView(h, descr, m, org);
                h._SECTION();
                h._ARTICLE();

                h.NAV_("uk-card-footer");
                var wc = h.Web;
          
                h.DIV_("uk-margin-auto-left uk-button-group");
                h.VARTOOLS(m.id, nav: false);
                var disp = !(wrk is PubLotWork) || today <= m.ended;
                var capt = wrk is MyLotWork ? "我的" + descr.act : wrk is OrglyLotWork ? descr.act + "情况" : descr.act;
                h.VARTOOL(m.id, nameof(LotVarWork.act), subscript: m.IsProduct && org.IsInternal ? 0 : 1, capt, tip: m.name, enabled: disp);
                h._DIV();
                h._NAV();

                h._FORM();
            }
            h._MAIN();
        }

        public static void ViewLotTop(this HtmlContent h, Purchase off, string icon, string img)
        {
            h.SECTION_("uk-flex");
            h.PIC_("uk-width-1-2 uk-margin-auto-vertical");
            if (off.HasIcon)
            {
                h.T(nameof(icon));
            }
            else // symbolic icon
            {
                h.T("/lot-").T(off.typ).T(".png");
            }
            h._PIC();
            h.PIC_("uk-width-1-2 uk-margin-auto-vertical");
            if (off.HasImg)
            {
                h.T(nameof(img));
            }
            else // symbolic icon
            {
                h.T("/lot-").T(off.typ).T(".png");
            }
            h._PIC();
            h._SECTION();
            h.ALERT_("uk-margin-remove-top").T(off.tip)._ALERT();
        }

        public static string FormatLotTime(DateTime t)
        {
            var sb = new StringBuilder();

            sb.Append(t.Year).Append('-');

            var mon = t.Month;
            if (mon < 10)
            {
                sb.Append('0');
            }
            sb.Append(mon).Append('-');

            var day = t.Day;
            if (day < 10)
            {
                sb.Append('0');
            }
            sb.Append(day).Append(' ');

            var hr = t.Hour;
            if (hr < 10)
            {
                sb.Append('0');
            }
            sb.Append(hr).Append(':');

            var min = t.Minute;
            if (min < 10)
            {
                sb.Append('0');
            }
            sb.Append(min);

            return sb.ToString();
        }

        public static async Task<bool> AddLotJnAsync(this DbContext dc, int lotid, int uid, decimal cash, Map<short, Org> orgs)
        {
            dc.Sql("UPDATE lotjns SET status =  pay = @1 WHERE lotid = @2 AND uid = @3 AND status = RETURNING qty, pay");
            if (!await dc.QueryTopAsync(p => p.Set(cash).Set(lotid).Set(uid)))
            {
                return false;
            }
            dc.Let(out short qty);
            dc.Let(out decimal pay);

            dc.Sql("UPDATE lots SET qtys = qtys + @1, pays = pays + @2 WHERE id = @3 AND status = ").T(STATUS_DRAFT).T(" RETURNING typ, orgid, name, min, qtys");
            if (!await dc.QueryTopAsync(p => p.Set(qty).Set(pay).Set(lotid)))
            {
                return false;
            }
            dc.Let(out short typ);
            dc.Let(out short orgid);
            dc.Let(out string name);
            dc.Let(out short min);
            dc.Let(out short qtys);

            // notify if meet mininal 
            if (qty >= min - (qtys % min))
            {
                var descr = LotDescr.All[typ];
                var org = orgs[orgid];
                if (!string.IsNullOrEmpty(org.Im))
                {
                    await WeChatUtility.PostSendAsync(org.Im, "【" + LOTS + "】" + name + "有" + descr.act + "（" + org.name + "）");
                }
            }
            return true;
        }


        private const string
            LOTJN_NOT_FOUND = "LOTJN_NOT_FOUND",
            LOT_NOT_FOUND = "LOT_NOT_FOUND";

        public static async Task<string> RemoveLotJnAsync(this DbContext dc, int lotid, int uid, string reason = null)
        {
            dc.Sql("DELETE FROM lotjns WHERE lotid = @1 AND uid = @2 AND status = RETURNING qty, inited, pay");
            if (!await dc.QueryTopAsync(p => p.Set(lotid).Set(uid)))
            {
                return LOTJN_NOT_FOUND;
            }
            dc.Let(out short qty);
            dc.Let(out DateTime inited);
            dc.Let(out decimal pay);

            dc.Sql("UPDATE lots SET qtys = qtys - @1, pays = pays - @2 WHERE id = @3 AND status < ").T(STATUS_ISSUED);
            if (await dc.ExecuteAsync(p => p.Set(qty).Set(pay).Set(lotid)) < 1)
            {
                return LOT_NOT_FOUND;
            }

            // refund
            if (pay > 0.00M)
            {
            }
            return null;
        }

        public static async Task<bool> SucceedLotAsync(this DbContext dc, int lotid, Map<short, Org> orgs)
        {
            // update status of the master record
            dc.Sql("UPDATE lots SET status = ").T(STATUS_ISSUED).T(" WHERE id = @1 AND status = ").T(STATUS_DRAFT).T(" RETURNING typ, orgid, name");
            if (!await dc.QueryTopAsync(p => p.Set(lotid)))
            {
                return false;
            }
            dc.Let(out short typ);
            dc.Let(out short orgid);
            dc.Let(out string name);

            var org = orgs[orgid];
            var descr = LotDescr.All[typ];

            await WeChatUtility.PostSendAsync(org.Im, "【" + LOTS + "】" + name + descr.act + "截止" + "，请做相关安排（" + org.name + "）");

            // notify all joiners
            dc.Sql("SELECT uim FROM lotjns WHERE lotid = @1 AND status = ");
            await dc.QueryAsync(p => p.Set(lotid));
            while (dc.Next())
            {
                dc.Let(out string uim);
                await WeChatUtility.PostSendAsync(uim, "【" + LOTS + "】" + name + "拼团完成" + "（" + org.name + "）");
            }

            return true;
        }

        public static async Task<bool> AbortLotAsync(this DbContext dc, int lotid, string reason, Map<short, Org> orgs)
        {
            // update master status
            //
            dc.Sql("UPDATE lots SET status = ").T(STATUS_ABORTED).T(", qtys = 0, pays = 0 WHERE id = @1 AND status < ").T(STATUS_CLOSED).T(" RETURNING typ, orgid, name");
            if (!await dc.QueryTopAsync(p => p.Set(lotid)))
            {
                return false;
            }
            dc.Let(out short typ);
            dc.Let(out short orgid);
            dc.Let(out string name);

            var org = orgs[orgid];

            await WeChatUtility.PostSendAsync(org.Im, "【" + LOTS + "】" + name + reason + "（" + org.name + "）");

            // delete all joiners
            //
            dc.Sql("DELETE FROM lotjns WHERE lotid = @1 AND status = RETURNING uid, uim, inited, pay");
            await dc.QueryAsync(p => p.Set(lotid));
            while (dc.Next())
            {
                dc.Let(out int uid);
                dc.Let(out string uim);
                dc.Let(out DateTime inited);
                dc.Let(out decimal pay);

                var sb = new StringBuilder();
                sb.Append("【").Append(LOTS).Append("】");
                sb.Append(name).Append(reason);
                sb.Append("（").Append(org.name).Append("）");

                // refund
                if (pay > 0.00M)
                {
                  
                }

                await WeChatUtility.PostSendAsync(uim, sb.ToString());
            }

            return true;
        }
    }
}