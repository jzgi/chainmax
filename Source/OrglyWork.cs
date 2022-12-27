using ChainFx;
using ChainFx.Web;
using static ChainFx.Fabric.Nodality;

namespace ChainMart
{
    public abstract class OrglyWork : WebWork
    {
    }

    /// <summary>
    /// Works for zones and sources.
    /// </summary>
    [UserAuthenticate]
    [Ui("供区产源操作")]
    public class ZonlyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<ZonlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }

    /// <summary>
    /// Works for markets and shops.
    /// </summary>
#if ZHNT
    [Ui("市场商户操作")]
#else
    [Ui("驿站商户操作")]
#endif
    [UserAuthenticate]
    public class MktlyWork : OrglyWork
    {
        protected override void OnCreate()
        {
            // id of either current user or the specified
            CreateVarWork<MktlyVarWork>((prin, key) =>
                {
                    var orgid = key?.ToInt() ?? ((User) prin).orgid;
                    return GrabObject<int, Org>(orgid);
                }
            );
        }
    }
}