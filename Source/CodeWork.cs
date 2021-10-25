﻿using System.Threading.Tasks;
using SkyChain.Web;

namespace Rev.Supply
{
    public class PublyCodeWork : WebWork
    {
        protected override void OnMake()
        {
            MakeVarWork<PublyCodeVarWork>();
        }

        public async Task @default(WebContext wc, int page)
        {
        }
    }
}