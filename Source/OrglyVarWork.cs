﻿using System.Threading.Tasks;
using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class OrglyVarWork : WebWork
    {
        [UserAuthorize(orgly: 15)]
        [Ui("操作权限"), Tool(Modal.ButtonOpen)]
        public async Task acl(WebContext wc, int cmd)
        {
            var org = wc[0].As<Org>();
            short orgly = 0;
            int id = 0;

            // retrieve the access list
            using var dc = NewDbContext();
            dc.Sql("SELECT name,tel,orgly FROM users WHERE orgid = @1 AND orgly > 0");
            var arr = dc.Query<Aclet>(p => p.Set(org.id));
            if (org.mgrid > 0) // append the org mgr
            {
                arr.AddOf(new Aclet
                {
                    id = org.mgrid,
                    name = org.mgrname,
                    tel = org.mgrtel,
                    orgly = 255
                });
            }

            if (wc.IsGet)
            {
                string tel = wc.Query[nameof(tel)];
                wc.GivePage(200, h =>
                {
                    h.TABLE(arr, o =>
                    {
                        h.TD_().T(o.name).SP().SUB(o.tel)._TD();
                        h.TD(User.Orgly[o.orgly]);
                        h.TDFORM(() =>
                        {
                            h.HIDDEN(nameof(id), o.id);
                            h.TOOL(nameof(acl), caption: "✕", subscript: 2, tool: ToolAttribute.BUTTON_CONFIRM, css: "uk-button-secondary");
                        });
                    }, caption: "现有权限");

                    h.FORM_().FIELDSUL_("授权给用户");
                    if (cmd == 0)
                    {
                        h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(acl), 1, post: false, css: "uk-button-secondary")._LI();
                    }
                    else if (cmd == 1) // find the user by tel
                    {
                        dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE tel = @1");
                        var o = dc.QueryTop<User>(p => p.Set(tel));
                        if (o != null)
                        {
                            h.LI_("uk-flex").TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true).BUTTON("查找", nameof(acl), 1, post: false, css: "uk-button-secondary")._LI();
                            h.LI_().FIELD("用户姓名", o.name)._LI();
                            h.LI_().SELECT("权限", nameof(orgly), orgly, User.Orgly, filter: (k, v) => k > 0, required: true)._LI();
                            h.LI_("uk-flex uk-flex-center").BUTTON("确认", nameof(acl), 2)._LI();
                            h.HIDDEN(nameof(o.id), o.id);
                        }
                    }
                    h._FIELDSUL()._FORM();
                }, false, 3);
            }
            else
            {
                var f = await wc.ReadAsync<Form>();
                id = f[nameof(id)];
                orgly = f[nameof(orgly)];
                dc.Execute("UPDATE users SET orgid = @1, orgly = @2 WHERE id = @3", p => p.Set(org.id).Set(orgly).Set(id));

                wc.GiveRedirect(nameof(acl)); // ok
            }
        }

        [Ui("运行设置"), Tool(Modal.ButtonShow)]
        public async Task setg(WebContext wc)
        {
            var org = wc[0].As<Org>();
            if (wc.IsGet)
            {
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("修改基本设置");
                    h.LI_().TEXT("标语", nameof(org.tip), org.tip, max: 16)._LI();
                    h.LI_().TEXT("地址", nameof(org.addr), org.addr, max: 16)._LI();
                    h.LI_().SELECT("状态", nameof(org.status), org.status, Entity.States, filter: (k, v) => k > 0)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else
            {
                var o = await wc.ReadObjectAsync(instance: org); // use existing object
                using var dc = NewDbContext();
                // update the db record
                await dc.ExecuteAsync("UPDATE orgs SET tip = @1, cttid = CASE WHEN @2 = 0 THEN NULL ELSE @2 END, date = @3 WHERE id = @4",
                    p => p.Set(o.tip).Set(o.status).Set(org.id));

                wc.GivePane(200);
            }
        }
    }

    [UserAuthorize(Org.TYP_SHP, 1)]
#if ZHNT
    [Ui("市场业务操作")]
#else
    [Ui("驿站业务操作")]
#endif
    public class MrtlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            // market

            CreateWork<MrtlyOrgWork>("morg");

            CreateWork<MrtlyBookWork>("mbook");

            CreateWork<MrtlyBuyWork>("mbuy");

            CreateWork<MrtlyCustWork>("mcust");

            // shop

            CreateWork<ShplyBookWork>("sbook");

            CreateWork<ShplyBuyWork>("sbuy");

            CreateWork<ShplyItemWork>("sitem");

            // common

            CreateWork<OrglyClearWork>("clear");

            CreateWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            wc.GivePage(200, h =>
            {
                var role = prin.orgid != org.id ? "代办" : User.Orgly[prin.orgly];
                // h.TOOLBAR(tip: prin.name + "（" + role + "）");

                h.TOPBAR_(true);
                if (prin.icon)
                {
                    h.PIC("/user/", prin.id, "/icon/", circle: true, css: "uk-width-medium");
                }
                else
                {
                    h.PIC("/org.webp", circle: true, css: "uk-width-small");
                }
                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.SPAN_().T(org.name).SP().SUB(Org.Typs[org.typ])._SPAN();
                h.SPAN(org.tel);
                h._DIV();
                h._TOPBAR(true);


                // h.FORM_("uk-card uk-card-default");
                // h.UL_("uk-card-body uk-list uk-list-divider");
                // h.LI_().FIELD2("机构名称", org.name, Org.Typs[org.typ], true)._LI();
                // h.LI_().FIELD(org.IsMrt ? "地址" : "编址", org.addr)._LI();
                // if (org.sprid > 0)
                // {
                //     var spr = GrabObject<int, Org>(org.sprid);
                //     h.LI_().FIELD("所在市场", spr.name)._LI();
                // }
                // if (org.ctrties != null)
                // {
                //     var ctr = GrabObject<int, Org>(org.ctrties[0]);
                //     h.LI_().FIELD("关联中控", ctr.name)._LI();
                // }
                // if (org.IsBiz)
                // {
                //     h.LI_().FIELD("委托代办", org.trust)._LI();
                // }
                // h._UL();
                // h._FORM();

                h.TASKLIST();
            }, false, 3);
        }
    }

    [UserAuthorize(Org.TYP_SRC, 1)]
    [Ui("供应产源操作")]
    public class PrvlyVarWork : OrglyVarWork
    {
        protected override void OnCreate()
        {
            CreateWork<PrvlyOrgWork>("porg");

            CreateWork<PrvlyRptWork>("prpt");


            CreateWork<SrclyProductWork>("sprod");

            CreateWork<SrclyLotWork>("slot");

            CreateWork<SrclyBookWork>("sbook");

            CreateWork<SrclyRptWork>("srpt");


            CreateWork<CtrlyLotWork>("clot");

            CreateWork<CtrlyBookWork>("cbook");

            CreateWork<CtrlyDistribWork>("cdistrib");

            CreateWork<CtrlyRptWork>("crpt");


            CreateWork<OrglyClearWork>("clear");

            CreateWork<OrglyAccessWork>("access");
        }

        public void @default(WebContext wc)
        {
            var org = wc[0].As<Org>();
            var topOrgs = Grab<int, Org>();
            var prin = (User) wc.Principal;

            wc.GivePage(200, h =>
            {
                h.TOPBAR_(true);
                if (org.icon)
                {
                    h.PIC("/org/", org.id, "/icon/", circle: true, css: "uk-width-medium");
                }
                else
                {
                    h.PIC("/org.png", circle: true, css: "uk-width-small");
                }
                h.DIV_("uk-width-expand uk-col uk-padding-small-left");
                h.H2(org.name);
                h.SPAN(org.tel);
                h._DIV();
                h._TOPBAR(true);

                h.TASKLIST();
            }, false, 3);
        }
    }
}