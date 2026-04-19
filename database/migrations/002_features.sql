-- ============================================================
-- Migration 002 – Shipping, Orders, Reviews
-- Run this after 001_init_marketplace.sql
-- ============================================================

-- ── Shipping methods reference table ──────────────────────────────────────────
create table if not exists catalog.shipping_methods (
    shipping_method_id int generated always as identity primary key,
    method_code        varchar(50)  not null unique,
    method_name        varchar(120) not null,
    description        text         null,
    is_active          boolean      not null default true,
    sort_order         int          not null default 0,
    created_at_utc     timestamptz  not null default now()
);

-- ── Listing shipping options (seller configures per listing) ──────────────────
create table if not exists marketplace.listing_shipping_options (
    listing_shipping_option_id uuid        primary key default gen_random_uuid(),
    listing_id                 uuid        not null references marketplace.listings(listing_id) on delete cascade,
    shipping_method_id         int         not null references catalog.shipping_methods(shipping_method_id),
    estimated_days             int         null,
    cost                       numeric(14,2) null,
    notes                      text        null,
    unique (listing_id, shipping_method_id)
);

-- ── Orders ─────────────────────────────────────────────────────────────────────
create table if not exists marketplace.orders (
    order_id           uuid          primary key default gen_random_uuid(),
    enquiry_id         uuid          null references messaging.enquiries(enquiry_id) on delete set null,
    buyer_user_id      uuid          not null references auth.users(user_id),
    seller_user_id     uuid          not null references auth.users(user_id),
    listing_id         uuid          not null references marketplace.listings(listing_id),
    quantity           numeric(14,2) not null,
    unit_price         numeric(14,2) not null,
    total_amount       numeric(14,2) not null,
    shipping_method_id int           null references catalog.shipping_methods(shipping_method_id),
    shipping_cost      numeric(14,2) not null default 0,
    order_status       varchar(30)   not null default 'PENDING',
    buyer_notes        text          null,
    seller_notes       text          null,
    created_at_utc     timestamptz   not null default now(),
    updated_at_utc     timestamptz   null
);

create table if not exists marketplace.order_status_history (
    order_status_history_id uuid        primary key default gen_random_uuid(),
    order_id                uuid        not null references marketplace.orders(order_id) on delete cascade,
    status_code             varchar(30) not null,
    note                    text        null,
    changed_by_user_id      uuid        null references auth.users(user_id),
    changed_at_utc          timestamptz not null default now()
);

-- ── Reviews ────────────────────────────────────────────────────────────────────
create table if not exists marketplace.reviews (
    review_id          uuid        primary key default gen_random_uuid(),
    listing_id         uuid        not null references marketplace.listings(listing_id) on delete cascade,
    reviewer_user_id   uuid        not null references auth.users(user_id),
    seller_user_id     uuid        not null references auth.users(user_id),
    rating             int         not null check (rating between 1 and 5),
    comment            text        null,
    is_visible         boolean     not null default true,
    created_at_utc     timestamptz not null default now(),
    unique (listing_id, reviewer_user_id)
);

-- ── Indexes ────────────────────────────────────────────────────────────────────
create index if not exists idx_orders_buyer        on marketplace.orders (buyer_user_id, created_at_utc desc);
create index if not exists idx_orders_seller       on marketplace.orders (seller_user_id, created_at_utc desc);
create index if not exists idx_orders_listing      on marketplace.orders (listing_id);
create index if not exists idx_orders_status       on marketplace.orders (order_status);
create index if not exists idx_reviews_listing     on marketplace.reviews (listing_id, created_at_utc desc);
create index if not exists idx_reviews_seller      on marketplace.reviews (seller_user_id);
create index if not exists idx_messages_conv       on messaging.messages (conversation_id, sent_at_utc desc);
create index if not exists idx_convs_buyer         on messaging.conversations (buyer_user_id);
create index if not exists idx_convs_seller        on messaging.conversations (seller_user_id);
create index if not exists idx_device_tokens_user  on messaging.device_tokens (user_id, is_active);
create index if not exists idx_listing_views_list  on marketplace.listing_views (listing_id, viewed_at_utc desc);
create index if not exists idx_otp_lookup          on auth.otp_codes (user_id, purpose, is_used, expires_at_utc);
create index if not exists idx_shipping_options    on marketplace.listing_shipping_options (listing_id);

-- ── Seed shipping methods ──────────────────────────────────────────────────────
insert into catalog.shipping_methods (method_code, method_name, description, sort_order) values
    ('PICKUP',       'Farm Gate Pickup',  'Buyer collects directly from the farm',           1),
    ('COURIER',      'Courier Delivery',  'Shipped via third-party courier to buyer',        2),
    ('OWN_DELIVERY', 'Seller Delivery',   'Seller delivers within the local area',           3),
    ('FREIGHT',      'Freight / Truck',   'Large bulk orders delivered by freight truck',    4)
on conflict (method_code) do nothing;
