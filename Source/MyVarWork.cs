using System.Threading.Tasks;
using SkyChain;
using SkyChain.Web;
using static System.String;
using static SkyChain.Web.Modal;

namespace Zhnt.Supply
{
    [UserAuthorize]
    [Ui("账号信息")]
    public class MyVarWork : WebWork
    {
        [UserAuthorize]
        public async Task @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            using var dc = NewDbContext();
            dc.Sql("SELECT ").collst(User.Empty).T(" FROM users WHERE id = @1");
            var o = dc.QueryTop<User>(p => p.Set(prin.id));

            // refresh cookie
            wc.SetTokenCookie(o);

            wc.GivePage(200, h =>
            {
                h.TOOLBAR(caption: prin.name);

                h.FORM_("uk-card uk-card-default");
                h.UL_("uk-card-body");
                h.LI_().FIELD("姓名", o.name)._LI();
                h.LI_().FIELD("手机号码", o.tel)._LI();
                h.LI_().FIELD("专业验证", User.Typs[o.typ])._LI();
                h._UL();
                h._FORM();

                h.FORM_();
                h.OPLIST();
                h._FORM();
            }, false, 3, title: "我的账户");
        }

        [Ui("设置"), Tool(ButtonShow)]
        public async Task setg(WebContext wc)
        {
            const string PASSMASK = "t#0^0z4R4pX7";
            string name;
            string tel;
            string password;
            var prin = (User) wc.Principal;
            if (wc.IsGet)
            {
                name = prin.name;
                tel = prin.tel;
                password = IsNullOrEmpty(prin.credential) ? null : PASSMASK;
                wc.GivePane(200, h =>
                {
                    h.FORM_().FIELDSUL_("基本信息");
                    h.LI_().TEXT("姓名", nameof(name), name, max: 8, min: 2, required: true)._LI();
                    h.LI_().TEXT("手机号码", nameof(tel), tel, pattern: "[0-9]+", max: 11, min: 11, required: true);
                    h._FIELDSUL().FIELDSUL_("操作密码（可选）");
                    h.LI_().PASSWORD("密码", nameof(password), password, max: 12, min: 3)._LI();
                    h._FIELDSUL()._FORM();
                });
            }
            else // POST
            {
                var f = await wc.ReadAsync<Form>();
                name = f[nameof(name)];
                tel = f[nameof(tel)];
                password = f[nameof(password)];
                string credential =
                    IsNullOrEmpty(password) ? null :
                    password == PASSMASK ? prin.credential :
                    SupplyUtility.ComputeCredential(tel, password);

                using var dc = NewDbContext();
                dc.Sql("UPDATE users SET name = CASE WHEN @1 IS NULL THEN name ELSE @1 END , tel = @2, credential = @3 WHERE id = @4 RETURNING ").collst(User.Empty);
                prin = await dc.QueryTopAsync<User>(p => p.Set(name).Set(tel).Set(credential).Set(prin.id));
                // refresh cookie
                wc.SetTokenCookie(prin);
                wc.GivePane(200); // close
            }
        }
    }
}