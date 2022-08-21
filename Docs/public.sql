create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create type purchop_type as
(
    state smallint,
    label varchar(12),
    orgid integer,
    uid integer,
    uname varchar(12),
    utel varchar(11),
    stamp timestamp(0)
);

alter type purchop_type owner to postgres;

create type buyln_type as
(
    stockid integer,
    name varchar(12),
    wareid smallint,
    price money,
    qty smallint,
    qtyre smallint
);

alter type buyln_type owner to postgres;

create table if not exists entities
(
    typ smallint not null,
    state smallint default 0 not null,
    name varchar(12) not null,
    tip varchar(30),
    created timestamp(0),
    creator varchar(10),
    adapted timestamp(0),
    adapter varchar(10)
);

alter table entities owner to postgres;

create table if not exists users
(
    id serial not null
        constraint users_pk
            primary key,
    tel varchar(11) not null,
    im varchar(28),
    credential varchar(32),
    admly smallint default 0 not null,
    orgid smallint,
    orgly smallint default 0 not null,
    idcard varchar(18),
    icon bytea
)
    inherits (entities);

alter table users owner to postgres;

create index if not exists users_admly_idx
    on users (admly)
    where (admly > 0);

create unique index if not exists users_im_idx
    on users (im);

create unique index if not exists users_tel_idx
    on users (tel);

create index if not exists users_orgid_idx
    on users (orgid);

create table if not exists regs
(
    id smallint not null
        constraint regs_pk
            primary key,
    idx smallint,
    num smallint
)
    inherits (entities);

comment on column regs.num is 'sub resources';

alter table regs owner to postgres;

create table if not exists orgs
(
    id serial not null
        constraint orgs_pk
            primary key,
    fork smallint,
    sprid integer
        constraint orgs_sprid_fk
            references orgs,
    license varchar(20),
    trust boolean,
    regid smallint
        constraint orgs_regid_fk
            references regs
            on update cascade,
    addr varchar(30),
    x double precision,
    y double precision,
    tel varchar(11),
    mgrid integer
        constraint orgs_mgrid_fk
            references users,
    ctrid integer,
    icon bytea
)
    inherits (entities);

alter table orgs owner to postgres;

alter table users
    add constraint users_orgid_fk
        foreign key (orgid) references orgs;

create table if not exists dailys
(
    orgid integer,
    dt date,
    itemid smallint,
    count integer,
    amt money,
    qty integer
)
    inherits (entities);

alter table dailys owner to postgres;

create table if not exists ledgers_
(
    seq integer,
    acct varchar(20),
    name varchar(12),
    amt integer,
    bal integer,
    cs uuid,
    blockcs uuid,
    stamp timestamp(0)
);

alter table ledgers_ owner to postgres;

create table if not exists peerledgs_
(
    peerid smallint
)
    inherits (ledgers_);

alter table peerledgs_ owner to postgres;

create table if not exists peers_
(
    id smallint not null
        constraint peers_pk
            primary key,
    weburl varchar(50),
    secret varchar(16)
)
    inherits (entities);

alter table peers_ owner to postgres;

create table if not exists accts_
(
    no varchar(20),
    v integer
)
    inherits (entities);

alter table accts_ owner to postgres;

create table if not exists notes
(
    id serial not null,
    fromid integer,
    toid integer
)
    inherits (entities);

comment on table notes is 'annoucements and notices';

alter table notes owner to postgres;

create table if not exists buys
(
    id bigserial not null
        constraint buys_pk
            primary key,
    shpid integer not null,
    mrtid integer not null,
    uid integer not null,
    uname varchar(10),
    utel varchar(11),
    uaddr varchar(20),
    uim varchar(28),
    lns buyln_type[],
    pay money,
    payre money,
    status smallint
)
    inherits (entities);

comment on table buys is 'customer buys';

alter table buys owner to postgres;

create table if not exists clears
(
    id serial not null
        constraint clears_pk
            primary key,
    dt date,
    orgid integer not null,
    sprid integer not null,
    orders integer,
    total money,
    rate money,
    pay integer,
    status smallint
)
    inherits (entities);

alter table clears owner to postgres;

create table if not exists cats
(
    idx smallint,
    num smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (entities);

comment on column cats.num is 'sub resources';

alter table cats owner to postgres;

create table if not exists products
(
    id serial not null
        constraint products_pk
            primary key,
    srcid integer
        constraint products_srcid_fk
            references orgs,
    ext varchar(10),
    store smallint,
    duration smallint,
    agt boolean,
    unit varchar(4),
    unitip varchar(12),
    price money,
    "off" money,
    icon bytea,
    pic bytea,
    constraint products_typ_fk
        foreign key (typ) references cats
)
    inherits (entities);

comment on column products.store is 'storage method';

comment on column products.unitip is 'ratio to standard unit';

alter table products owner to postgres;

create table if not exists items
(
    id serial not null
        constraint items_pk
            primary key,
    shpid integer,
    productid integer,
    unit varchar(4),
    unitx smallint,
    price money,
    min smallint,
    max smallint,
    step smallint,
    icon bytea,
    pic bytea
)
    inherits (entities);

alter table items owner to postgres;

create table if not exists books
(
    id bigserial not null
        constraint books_pk
            primary key,
    shpid integer not null,
    shpop varchar(12),
    shpon timestamp(0),
    srcid integer not null,
    srcop varchar(12),
    srcon timestamp(0),
    prvid integer not null,
    prvpop varchar(12),
    prvon timestamp(0),
    ctrid integer not null,
    ctrop varchar(12),
    ctron timestamp(0),
    mrtid integer not null,
    mrtop varchar(12),
    mrton timestamp(0),
    productid integer,
    unit varchar(4),
    unitx smallint,
    shipat date,
    price money,
    "off" money,
    qty integer,
    qtyre integer,
    pay money,
    payre money,
    status smallint
)
    inherits (entities);

comment on column books.unitx is 'ratio to standard unit';

comment on column books.qtyre is 'qty reduced';

comment on column books.payre is 'pay refunded';

alter table books owner to postgres;

create table if not exists distribs
(
    id serial not null,
    productid integer,
    srcid integer,
    srcop varchar(12),
    srcon timestamp(0),
    prvid integer,
    prvop varchar(12),
    prvon timestamp(0),
    ctrid integer,
    ctrop varchar(12),
    ctron timestamp(0),
    ownid integer,
    price money,
    "off" money,
    cap integer,
    remain integer,
    min integer,
    max integer,
    step integer,
    status smallint
)
    inherits (entities);

alter table distribs owner to postgres;

create or replace view users_vw(typ, state, name, tip, created, creator, adapted, adapter, id, tel, im, credential, admly, orgid, orgly, idcard, icon) as
SELECT u.typ,
       u.state,
       u.name,
       u.tip,
       u.created,
       u.creator,
       u.adapted,
       u.adapter,
       u.id,
       u.tel,
       u.im,
       u.credential,
       u.admly,
       u.orgid,
       u.orgly,
       u.idcard,
       u.icon IS NOT NULL AS icon
FROM users u;

alter table users_vw owner to postgres;

create or replace view orgs_vw(typ, state, name, tip, created, creator, adapted, adapter, id, fork, sprid, license, trust, regid, addr, x, y, tel, ctrid, mgrid, mgrname, mgrtel, mgrim, icon) as
SELECT o.typ,
       o.state,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.id,
       o.fork,
       o.sprid,
       o.license,
       o.trust,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.ctrid,
       o.mgrid,
       m.name             AS mgrname,
       m.tel              AS mgrtel,
       m.im               AS mgrim,
       o.icon IS NOT NULL AS icon
FROM orgs o
         LEFT JOIN users m
                   ON o.mgrid =
                      m.id;

alter table orgs_vw owner to postgres;

create or replace function first_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as $$
SELECT $1
$$;

alter function first_agg(anyelement, anyelement) owner to postgres;

create or replace function last_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as $$
SELECT $2
$$;

alter function last_agg(anyelement, anyelement) owner to postgres;

create aggregate first(anyelement) (
    sfunc = first_agg,
    stype = anyelement,
    parallel = safe
    );

alter aggregate first(anyelement) owner to postgres;

create aggregate last(anyelement) (
    sfunc = last_agg,
    stype = anyelement,
    parallel = safe
    );

alter aggregate last(anyelement) owner to postgres;

