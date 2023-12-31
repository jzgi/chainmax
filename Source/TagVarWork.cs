﻿using System.Threading.Tasks;
using ChainFx.Web;
using static ChainFx.Nodal.Nodality;

namespace ChainSmart;

public abstract class TagVarWork : WebWork
{
}

public class PublyTagVarWork : TagVarWork
{
    public async Task @default(WebContext wc)
    {
        int id = wc[0];

        using var dc = NewDbContext();
        dc.Sql("SELECT id FROM lots_vw WHERE nend >= @1 AND nstart <= @1 ORDER BY nend ASC LIMIT 1");
        if (await dc.QueryTopAsync(p => p.Set(id)))
        {
            dc.Let(out int lotid);
            wc.GiveRedirect("/lot/" + lotid + "/");
        }
        else
        {
            wc.GivePage(304, h => h.ALERT("此溯源码没有绑定产品"));
        }
    }
}