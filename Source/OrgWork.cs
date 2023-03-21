using System;
using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Web.Modal;
using static ChainFx.Nodal.Nodality;
using static ChainFx.Web.ToolAttribute;

namespace ChainSmart
{
    public abstract class OrgWork<V> : WebWork where V : OrgVarWork, new()
    {
        protected override void OnCreate()
        {
            CreateVarWork<V>();
        }
    }

    public class PublyOrgWork : OrgWork<PublyOrgVarWork>
    {
    }

    [AdmlyAuthorize(User.ROL_OPN)]
    [Ui("机构管理", "业务")]
    public class AdmlyOrgWork : OrgWork<AdmlyOrgVarWork>
    {
        protected static void MainGrid(HtmlBuilder h, Org[] arr, User prin, bool shply)
        {
            h.MAINGRID(arr, o =>
            {
                h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                if (o.icon)
                {
                    h.PIC(MainApp.WwwUrl, "/org/", o.id, "/icon", css: "uk-width-1-5");
                }
                else
                    h.PIC("/void.webp", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.name).SPAN(Org.Statuses[o.status], "uk-badge")._HEADER();
                h.Q2(o.Ext, o.tip, css: "uk-width-expand");
                h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR((shply ? "/shply/" : "/srcly/"), o.Key, "/", icon: "link", disabled: !prin.CanSupervize(o))._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }

        [Ui("市场机构", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var prin = (User)wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ = ").T(Org.TYP_MKT).T(" ORDER BY regid, status DESC");
            var arr = await dc.QueryAsync<Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 1);
                if (arr == null)
                {
                    h.ALERT("尚无市场机构");
                    return;
                }

                MainGrid(h, arr, prin, true);
            }, false, 12);
        }

        [Ui("供应机构", group: 2), Tool(Anchor)]
        public async Task ctr(WebContext wc)
        {
            var prin = (User)wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE typ = ").T(Org.TYP_CTR).T(" ORDER BY status DESC");
            var arr = await dc.QueryAsync<Org>();

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: 2);
                if (arr == null)
                {
                    h.ALERT("尚无供应机构");
                    return;
                }

                MainGrid(h, arr, prin, true);
            }, false, 12);
        }

        [Ui("新建", "新建机构", icon: "plus", group: 3), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int cmd)
        {
            var prin = (User)wc.Principal;
            var regs = Grab<short, Reg>();
            var orgs = Grab<int, Org>();

            var m = new Org
            {
                typ = cmd == 1 ? Org.TYP_MKT : Org.TYP_CTR,
                created = DateTime.Now,
                creator = prin.name,
            };

            if (wc.IsGet)
            {
                m.Read(wc.Query, 0);

                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_(cmd == 1 ? "市场机构" : "供应机构");

                    h.LI_().TEXT("商户名", nameof(m.name), m.name, min: 2, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().TEXT("工商登记名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                    h.LI_().TEXT("范围延展名", nameof(m.ext), m.ext, max: 12, required: true)._LI();
                    h.LI_().SELECT("地市", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsCity, required: true)._LI();
                    h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 30)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    if (cmd == 1)
                    {
                        h.LI_().SELECT("关联中库", nameof(m.ctrid), m.ctrid, orgs, filter: (_, v) => v.EqCenter, required: true)._LI();
                    }

                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;

                var o = await wc.ReadObjectAsync(msk, m);

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, Entity.MSK_BORN));

                wc.GivePane(201); // created
            }
        }
    }

    [OrglyAuthorize(Org.TYP_MKT)]
    [Ui("成员商户", "机构")]
    public class MktlyOrgWork : OrgWork<MktlyOrgVarWork>
    {
        static void MainGrid(HtmlBuilder h, Org[] arr, User prin)
        {
            h.MAINGRID(arr, o =>
            {
                h.ADIALOG_(o.Key, "/-", o.typ, MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                if (o.icon)
                {
                    h.PIC(MainApp.WwwUrl, "/org/", o.id, "/icon", css: "uk-width-1-5");
                }
                else
                    h.PIC("/void.webp", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.name).SPAN("")._HEADER();
                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/shply/", o.Key, "/", icon: "link", disabled: !prin.CanSupervize(o))._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }

        [Ui("成员商户", group: 1), Tool(Anchor)]
        public async Task @default(WebContext wc, int page)
        {
            var org = wc[-1].As<Org>();
            var prin = (User)wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 ORDER BY id DESC LIMIT 20 OFFSET @2 * 20");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id).Set(page));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Org.TYP_SHP);

                if (arr == null)
                {
                    h.ALERT("尚无成员商户");
                    return;
                }

                MainGrid(h, arr, prin);

                h.PAGINATION(arr.Length == 20);
            }, false, 15);
        }

        [Ui(tip: "查询", icon: "search", group: 2), Tool(AnchorPrompt)]
        public async Task search(WebContext wc)
        {
            var regs = Grab<short, Reg>();
            var prin = (User)wc.Principal;

            bool inner = wc.Query[nameof(inner)];
            short regid = 0;
            if (inner)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_();
                    h.RADIOSET(nameof(regid), regid, regs, filter: v => v.IsSection);
                    h._FORM();
                });
            }
            else // OUTER
            {
                regid = wc.Query[nameof(regid)];

                using var dc = NewDbContext();
                dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE regid = @1");
                var arr = await dc.QueryAsync<Org>(p => p.Set(regid));

                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();

                    if (arr == null)
                    {
                        h.ALERT("尚无该组成员");
                        return;
                    }

                    MainGrid(h, arr, prin);
                }, false, 15);
            }
        }

        [Ui(tip: "品牌链接", icon: "star", group: 4), Tool(Anchor)]
        public async Task star(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User)wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND typ = 0 ORDER BY id DESC");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(subscript: Org.TYP_BRD);

                if (arr == null)
                {
                    h.ALERT("尚无成员品牌");
                    return;
                }

                MainGrid(h, arr, prin);
            }, false, 15);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("新建", "新建成员商户", icon: "plus", group: 1 | 4), Tool(ButtonOpen)]
        public async Task @new(WebContext wc, int typ)
        {
            var org = wc[-1].As<Org>();
            var prin = (User)wc.Principal;

            var regs = Grab<short, Reg>();
            var o = new Org
            {
                typ = (short)typ,
                created = DateTime.Now,
                creator = prin.name,
                prtid = org.id,
                ctrid = org.ctrid,
            };

            if (wc.IsGet)
            {
                if (typ == Org.TYP_SHP)
                {
                    o.Read(wc.Query, 0);
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("商户信息");

                        h.LI_().TEXT("商户名", nameof(o.name), o.name, max: 12, required: true)._LI();
                        h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                        h.LI_().TEXT("工商登记名", nameof(o.legal), o.legal, max: 20, required: true)._LI();
                        h.LI_().TEXT("联系电话", nameof(o.tel), o.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                        h.LI_().SELECT("场区", nameof(o.regid), o.regid, regs, filter: (_, v) => v.IsSection)._LI();
                        h.LI_().TEXT("商户编号", nameof(o.addr), o.addr, max: 4)._LI();
                        // h.LI_().TEXT("地址", nameof(m.addr), m.addr, max: 20)._LI();
                        // h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.000, max: 180.000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                        h.LI_().CHECKBOX("委托办理", nameof(o.trust), true, o.trust)._LI();

                        h._FIELDSUL()._FORM();
                    });
                }
                else // TYP_VTL
                {
                    o.Read(wc.Query, 0);
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDSUL_("填写虚拟商户信息");

                        h.LI_().TEXT("名称", nameof(o.name), o.name, max: 12, required: true)._LI();
                        h.LI_().TEXTAREA("简介", nameof(o.tip), o.tip, max: 40)._LI();
                        h.LI_().TEXT("链接地址", nameof(o.addr), o.addr, max: 50)._LI();
                        // h.LI_().SELECT("状态", nameof(o.state), o.state, Entity.States, filter: (k, v) => k >= 0)._LI();

                        h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
                    });
                }
            }
            else // POST
            {
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                await wc.ReadObjectAsync(msk, instance: o);

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, msk));

                wc.GivePane(201); // created
            }
        }
    }

    [OrglyAuthorize(Org.TYP_CTR)]
    [Ui("成员商户", "机构")]
    public class CtrlyOrgWork : OrgWork<CtrlyOrgVarWork>
    {
        static void MainGrid(HtmlBuilder h, Org[] arr, User prin)
        {
            h.MAINGRID(arr, o =>
            {
                h.ADIALOG_(o.Key, "/", MOD_OPEN, false, tip: o.name, css: "uk-card-body uk-flex");

                if (o.icon)
                {
                    h.PIC(MainApp.WwwUrl, "/org/", o.id, "/icon", css: "uk-width-1-5");
                }
                else
                    h.PIC("/void.webp", css: "uk-width-1-5");

                h.ASIDE_();
                h.HEADER_().H4(o.name).SPAN(Org.Statuses[o.status], "uk-badge")._HEADER();
                h.Q(o.tip, "uk-width-expand");
                h.FOOTER_().SPAN_("uk-margin-auto-left").BUTTONVAR("/srcly/", o.Key, "/", icon: "link", disabled: !prin.CanSupervize(o))._SPAN()._FOOTER();
                h._ASIDE();

                h._A();
            });
        }

        [Ui("成员商户"), Tool(Anchor)]
        public async Task @default(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User)wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND status = 4 ORDER BY oked DESC");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无上线商户");
                    return;
                }

                MainGrid(h, arr, prin);
            }, false, 12);
        }

        [Ui(tip: "已下线", icon: "cloud-download"), Tool(Anchor)]
        public async Task down(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User)wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND status BETWEEN 1 AND 2 ORDER BY oked DESC");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无下线商户");
                    return;
                }

                MainGrid(h, arr, prin);
            }, false, 15);
        }

        [Ui(tip: "已删除", icon: "trash"), Tool(Anchor)]
        public async Task @void(WebContext wc)
        {
            var org = wc[-1].As<Org>();
            var prin = (User)wc.Principal;

            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(Org.Empty).T(" FROM orgs_vw WHERE prtid = @1 AND status = 0 ORDER BY adapted DESC");
            var arr = await dc.QueryAsync<Org>(p => p.Set(org.id));

            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                if (arr == null)
                {
                    h.ALERT("尚无删除商户");
                    return;
                }

                MainGrid(h, arr, prin);
            }, false, 15);
        }

        [OrglyAuthorize(0, User.ROL_OPN)]
        [Ui("新建", "新建成员商户", icon: "plus"), Tool(ButtonOpen)]
        public async Task @new(WebContext wc)
        {
            var zon = wc[-1].As<Org>();
            var prin = (User)wc.Principal;
            var regs = Grab<short, Reg>();
            var m = new Org
            {
                typ = Org.TYP_SRC,
                prtid = zon.id,
                created = DateTime.Now,
                creator = prin.name,
            };
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_();

                    h.LI_().TEXT("商户名", nameof(m.name), m.name, max: 12, required: true)._LI();
                    h.LI_().TEXTAREA("简介", nameof(m.tip), m.tip, max: 40)._LI();
                    h.LI_().TEXT("工商登记名", nameof(m.legal), m.legal, max: 20, required: true)._LI();
                    h.LI_().SELECT("省份", nameof(m.regid), m.regid, regs, filter: (_, v) => v.IsProvince, required: true)._LI();
                    h.LI_().TEXT("联系地址", nameof(m.addr), m.addr, max: 30)._LI();
                    h.LI_().NUMBER("经度", nameof(m.x), m.x, min: 0.0000, max: 180.0000).NUMBER("纬度", nameof(m.y), m.y, min: -90.000, max: 90.000)._LI();
                    h.LI_().TEXT("联系电话", nameof(m.tel), m.tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h.LI_().CHECKBOX("委托代办", nameof(m.trust), true, m.trust)._LI();

                    h._FIELDSUL().BOTTOM_BUTTON("确认", nameof(@new))._FORM();
                });
            }
            else // POST
            {
                const short msk = Entity.MSK_BORN | Entity.MSK_EDIT;
                var o = await wc.ReadObjectAsync(msk, instance: m);

                using var dc = NewDbContext();
                dc.Sql("INSERT INTO orgs ").colset(Org.Empty, msk)._VALUES_(Org.Empty, msk);
                await dc.ExecuteAsync(p => o.Write(p, msk));

                wc.GivePane(201); // created
            }
        }
    }
}