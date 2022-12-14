﻿using System;
using ChainFx;

namespace ChainMart
{
    /// <summary>
    /// A product lot for booking.
    /// </summary>
    public class Lot : Entity, IKeyable<int>
    {
        public static readonly Lot Empty = new Lot();

        public new static readonly Map<short, string> States = new Map<short, string>
        {
            {STA_VOID, null},
            {STA_NORMAL, "溯源"},
        };


        public new static readonly Map<short, string> Statuses = new Map<short, string>
        {
            {STU_CREATED, "创建"},
            {STU_ADAPTED, "调整"},
            {STU_OKED, "上线"},
        };

        internal int id;
        internal int srcid;
        internal string srcname;
        internal int zonid;
        internal int ctrid;
        internal int[] mktids; // optional

        // individual order relevant
        internal int itemid;
        internal string unit;
        internal string unitx; // proximate units
        internal DateTime futured;
        internal decimal price;
        internal decimal off;
        internal int cap;
        internal int remain;
        internal short min;
        internal short max;
        internal short step;

        internal int nstart;
        internal int nend;
        internal bool m1;
        internal bool m2;
        internal bool m3;
        internal bool m4;

        public override void Read(ISource s, short msk = 0xff)
        {
            base.Read(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Get(nameof(id), ref id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Get(nameof(srcid), ref srcid);
                s.Get(nameof(srcname), ref srcname);
                s.Get(nameof(zonid), ref zonid);
                s.Get(nameof(ctrid), ref ctrid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Get(nameof(itemid), ref itemid);
                s.Get(nameof(unit), ref unit);
                s.Get(nameof(unitx), ref unitx);
                s.Get(nameof(futured), ref futured);
                s.Get(nameof(price), ref price);
                s.Get(nameof(off), ref off);
                s.Get(nameof(min), ref min);
                s.Get(nameof(step), ref step);
                s.Get(nameof(max), ref max);
                s.Get(nameof(cap), ref cap);
                s.Get(nameof(remain), ref remain);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Get(nameof(mktids), ref mktids);
                s.Get(nameof(nstart), ref nstart);
                s.Get(nameof(nend), ref nend);
                s.Get(nameof(m1), ref m1);
                s.Get(nameof(m2), ref m2);
                s.Get(nameof(m3), ref m3);
                s.Get(nameof(m4), ref m4);
            }
        }

        public override void Write(ISink s, short msk = 0xff)
        {
            base.Write(s, msk);

            if ((msk & MSK_ID) == MSK_ID)
            {
                s.Put(nameof(id), id);
            }
            if ((msk & MSK_BORN) == MSK_BORN)
            {
                s.Put(nameof(srcid), srcid);
                s.Put(nameof(srcname), srcname);
                s.Put(nameof(zonid), zonid);
                s.Put(nameof(ctrid), ctrid);
            }
            if ((msk & MSK_EDIT) == MSK_EDIT)
            {
                s.Put(nameof(itemid), itemid);
                s.Put(nameof(unit), unit);
                s.Put(nameof(unitx), unitx);
                s.Put(nameof(futured), futured);
                s.Put(nameof(price), price);
                s.Put(nameof(off), off);
                s.Put(nameof(min), min);
                s.Put(nameof(step), step);
                s.Put(nameof(max), max);
                s.Put(nameof(cap), cap);
                s.Put(nameof(remain), remain);
            }
            if ((msk & MSK_LATER) == MSK_LATER)
            {
                s.Put(nameof(mktids), mktids);
                s.Put(nameof(nstart), nstart);
                s.Put(nameof(nend), nend);
                s.Put(nameof(m1), m1);
                s.Put(nameof(m2), m2);
                s.Put(nameof(m3), m3);
                s.Put(nameof(m4), m4);
            }
        }

        public int Key => id;

        public bool IsSelfTransport => mktids != null;

        public decimal RealPrice => price - off;


        public bool IsAvailableFor(int mktid)
        {
            return mktids == null || mktids.Contains(mktid);
        }

        public override string ToString() => name;
    }
}