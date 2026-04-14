create extension if not exists pgcrypto;

create schema if not exists auth;
create schema if not exists catalog;
create schema if not exists seller;
create schema if not exists marketplace;
create schema if not exists billing;
create schema if not exists messaging;
create schema if not exists audit;
create schema if not exists admin;
create schema if not exists integration;

create table if not exists auth.roles (
    role_id int generated always as identity primary key,
    role_code varchar(50) not null unique,
    role_name varchar(100) not null,
    created_at_utc timestamptz not null default now()
);

create table if not exists auth.users (
    user_id uuid primary key,
    full_name varchar(150) not null,
    email varchar(200) not null unique,
    phone varchar(20) not null unique,
    password_hash text not null,
    role_id int not null references auth.roles(role_id),
    is_active boolean not null default true,
    last_login_at_utc timestamptz null,
    created_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz null
);

create table if not exists auth.refresh_tokens (
    refresh_token_id uuid primary key,
    user_id uuid not null references auth.users(user_id),
    token text not null unique,
    expires_at_utc timestamptz not null,
    is_revoked boolean not null default false,
    revoked_at_utc timestamptz null,
    created_at_utc timestamptz not null default now()
);

create table if not exists auth.otp_codes (
    otp_code_id uuid primary key default gen_random_uuid(),
    user_id uuid not null references auth.users(user_id),
    purpose varchar(30) not null,
    code_hash text not null,
    expires_at_utc timestamptz not null,
    is_used boolean not null default false,
    created_at_utc timestamptz not null default now()
);

create table if not exists catalog.regions (
    region_id int generated always as identity primary key,
    region_name varchar(120) not null,
    is_active boolean not null default true
);

create table if not exists catalog.districts (
    district_id int generated always as identity primary key,
    region_id int not null references catalog.regions(region_id),
    district_name varchar(120) not null,
    is_active boolean not null default true
);

create table if not exists catalog.categories (
    category_id int generated always as identity primary key,
    category_code varchar(50) not null unique,
    category_name varchar(120) not null,
    is_active boolean not null default true
);

create table if not exists catalog.product_types (
    product_type_id int generated always as identity primary key,
    category_id int not null references catalog.categories(category_id),
    product_type_name varchar(120) not null,
    is_active boolean not null default true
);

create table if not exists catalog.units (
    unit_id int generated always as identity primary key,
    unit_code varchar(30) not null unique,
    unit_name varchar(80) not null
);

create table if not exists catalog.listing_statuses (
    listing_status_id int generated always as identity primary key,
    status_code varchar(30) not null unique,
    status_name varchar(80) not null
);

create table if not exists catalog.subscription_statuses (
    subscription_status_id int generated always as identity primary key,
    status_code varchar(30) not null unique,
    status_name varchar(80) not null
);

create table if not exists catalog.payment_statuses (
    payment_status_id int generated always as identity primary key,
    status_code varchar(30) not null unique,
    status_name varchar(80) not null
);

create table if not exists catalog.enquiry_statuses (
    enquiry_status_id int generated always as identity primary key,
    status_code varchar(30) not null unique,
    status_name varchar(80) not null
);

create table if not exists catalog.contact_modes (
    contact_mode_id int generated always as identity primary key,
    mode_code varchar(30) not null unique,
    mode_name varchar(80) not null
);

create table if not exists catalog.payment_methods (
    payment_method_id int generated always as identity primary key,
    method_code varchar(30) not null unique,
    method_name varchar(80) not null
);

create table if not exists seller.seller_profiles (
    seller_profile_id uuid primary key,
    user_id uuid not null unique references auth.users(user_id),
    business_name varchar(180) not null,
    description text null,
    region_id int not null references catalog.regions(region_id),
    district_id int not null references catalog.districts(district_id),
    contact_mode varchar(30) not null,
    profile_image_url text null,
    is_verified boolean not null default false,
    created_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz null
);

create table if not exists seller.seller_verifications (
    seller_verification_id uuid primary key default gen_random_uuid(),
    seller_profile_id uuid not null references seller.seller_profiles(seller_profile_id),
    verification_status varchar(30) not null,
    reviewed_by_user_id uuid null references auth.users(user_id),
    reviewed_at_utc timestamptz null,
    note text null,
    created_at_utc timestamptz not null default now()
);

create table if not exists seller.seller_addresses (
    seller_address_id uuid primary key default gen_random_uuid(),
    seller_profile_id uuid not null references seller.seller_profiles(seller_profile_id),
    line1 varchar(200) not null,
    line2 varchar(200) null,
    region_id int not null references catalog.regions(region_id),
    district_id int not null references catalog.districts(district_id),
    is_primary boolean not null default false,
    created_at_utc timestamptz not null default now()
);

create table if not exists seller.seller_documents (
    seller_document_id uuid primary key default gen_random_uuid(),
    seller_profile_id uuid not null references seller.seller_profiles(seller_profile_id),
    document_type varchar(60) not null,
    document_url text not null,
    verification_status varchar(30) not null,
    created_at_utc timestamptz not null default now()
);

create table if not exists messaging.buyer_profiles (
    buyer_profile_id uuid primary key,
    user_id uuid not null unique references auth.users(user_id),
    display_name varchar(150) not null,
    region_id int not null references catalog.regions(region_id),
    district_id int not null references catalog.districts(district_id),
    receive_sms boolean not null default true,
    receive_push boolean not null default true,
    created_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz null
);

create table if not exists marketplace.listings (
    listing_id uuid primary key,
    seller_user_id uuid not null references auth.users(user_id),
    title varchar(180) not null,
    description text not null,
    category_id int not null references catalog.categories(category_id),
    product_type_id int not null references catalog.product_types(product_type_id),
    price numeric(14,2) not null,
    quantity numeric(14,2) not null,
    unit_id int not null references catalog.units(unit_id),
    region_id int not null references catalog.regions(region_id),
    district_id int not null references catalog.districts(district_id),
    listing_status_id int not null references catalog.listing_statuses(listing_status_id),
    is_livestock boolean not null default false,
    expires_at_utc timestamptz null,
    created_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz null
);

create table if not exists marketplace.listing_images (
    listing_image_id uuid primary key,
    listing_id uuid not null references marketplace.listings(listing_id) on delete cascade,
    image_url text not null,
    is_primary boolean not null default false,
    sort_order int not null default 0,
    created_at_utc timestamptz not null default now()
);

create table if not exists marketplace.listing_attributes (
    listing_attribute_id uuid primary key default gen_random_uuid(),
    listing_id uuid not null references marketplace.listings(listing_id) on delete cascade,
    attribute_key varchar(80) not null,
    attribute_value varchar(300) not null,
    created_at_utc timestamptz not null default now()
);

create table if not exists marketplace.listing_status_history (
    listing_status_history_id uuid primary key,
    listing_id uuid not null references marketplace.listings(listing_id) on delete cascade,
    status_code varchar(30) not null,
    note text null,
    changed_by_user_id uuid null references auth.users(user_id),
    changed_at_utc timestamptz not null default now()
);

create table if not exists marketplace.listing_favorites (
    listing_favorite_id uuid primary key default gen_random_uuid(),
    listing_id uuid not null references marketplace.listings(listing_id) on delete cascade,
    buyer_user_id uuid not null references auth.users(user_id),
    created_at_utc timestamptz not null default now(),
    unique (listing_id, buyer_user_id)
);

create table if not exists marketplace.listing_views (
    listing_view_id uuid primary key default gen_random_uuid(),
    listing_id uuid not null references marketplace.listings(listing_id) on delete cascade,
    viewer_user_id uuid null references auth.users(user_id),
    viewed_at_utc timestamptz not null default now()
);

create table if not exists marketplace.crop_details (
    crop_detail_id uuid primary key default gen_random_uuid(),
    listing_id uuid not null unique references marketplace.listings(listing_id) on delete cascade,
    crop_variety varchar(120) null,
    harvest_date date null,
    organic_certified boolean not null default false
);

create table if not exists marketplace.livestock_details (
    livestock_detail_id uuid primary key default gen_random_uuid(),
    listing_id uuid not null unique references marketplace.listings(listing_id) on delete cascade,
    livestock_type varchar(120) not null,
    breed varchar(120) null,
    age_in_months int null,
    health_status varchar(120) null
);

create table if not exists billing.subscription_plans (
    plan_id int generated always as identity primary key,
    plan_name varchar(120) not null,
    price_amount numeric(14,2) not null,
    duration_days int not null,
    max_active_listings int not null,
    is_active boolean not null default true,
    sort_order int not null default 0,
    created_at_utc timestamptz not null default now()
);

create table if not exists billing.seller_subscriptions (
    seller_subscription_id uuid primary key,
    seller_user_id uuid not null references auth.users(user_id),
    plan_id int not null references billing.subscription_plans(plan_id),
    subscription_status_id int not null references catalog.subscription_statuses(subscription_status_id),
    start_date_utc timestamptz not null,
    end_date_utc timestamptz not null,
    created_at_utc timestamptz not null default now()
);

create table if not exists billing.subscription_payments (
    subscription_payment_id uuid primary key default gen_random_uuid(),
    seller_subscription_id uuid not null references billing.seller_subscriptions(seller_subscription_id),
    amount numeric(14,2) not null,
    payment_method_id int not null references catalog.payment_methods(payment_method_id),
    payment_status_id int not null references catalog.payment_statuses(payment_status_id),
    paid_at_utc timestamptz null,
    created_at_utc timestamptz not null default now()
);

create table if not exists billing.payment_transactions (
    payment_transaction_id uuid primary key default gen_random_uuid(),
    subscription_payment_id uuid null references billing.subscription_payments(subscription_payment_id),
    external_reference varchar(150) null,
    amount numeric(14,2) not null,
    payment_status_id int not null references catalog.payment_statuses(payment_status_id),
    response_payload jsonb null,
    created_at_utc timestamptz not null default now()
);

create table if not exists billing.invoices (
    invoice_id uuid primary key default gen_random_uuid(),
    seller_subscription_id uuid not null references billing.seller_subscriptions(seller_subscription_id),
    invoice_number varchar(60) not null unique,
    amount numeric(14,2) not null,
    issued_at_utc timestamptz not null default now(),
    due_at_utc timestamptz null,
    created_at_utc timestamptz not null default now()
);

create table if not exists messaging.enquiries (
    enquiry_id uuid primary key,
    listing_id uuid not null references marketplace.listings(listing_id) on delete cascade,
    buyer_user_id uuid not null references auth.users(user_id),
    seller_user_id uuid not null references auth.users(user_id),
    enquiry_status_id int not null references catalog.enquiry_statuses(enquiry_status_id),
    message text not null,
    preferred_contact_mode varchar(30) not null,
    created_at_utc timestamptz not null default now(),
    updated_at_utc timestamptz null
);

create table if not exists messaging.enquiry_status_history (
    enquiry_status_history_id uuid primary key,
    enquiry_id uuid not null references messaging.enquiries(enquiry_id) on delete cascade,
    status_code varchar(30) not null,
    note text null,
    changed_by_user_id uuid null references auth.users(user_id),
    changed_at_utc timestamptz not null default now()
);

create table if not exists messaging.conversations (
    conversation_id uuid primary key default gen_random_uuid(),
    enquiry_id uuid null references messaging.enquiries(enquiry_id) on delete set null,
    buyer_user_id uuid not null references auth.users(user_id),
    seller_user_id uuid not null references auth.users(user_id),
    created_at_utc timestamptz not null default now()
);

create table if not exists messaging.messages (
    message_id uuid primary key default gen_random_uuid(),
    conversation_id uuid not null references messaging.conversations(conversation_id) on delete cascade,
    sender_user_id uuid not null references auth.users(user_id),
    message_body text not null,
    sent_at_utc timestamptz not null default now(),
    is_read boolean not null default false
);

create table if not exists messaging.notifications (
    notification_id uuid primary key default gen_random_uuid(),
    user_id uuid not null references auth.users(user_id),
    title varchar(160) not null,
    body text not null,
    is_read boolean not null default false,
    created_at_utc timestamptz not null default now()
);

create table if not exists messaging.device_tokens (
    device_token_id uuid primary key default gen_random_uuid(),
    user_id uuid not null references auth.users(user_id),
    platform varchar(30) not null,
    token text not null,
    is_active boolean not null default true,
    created_at_utc timestamptz not null default now(),
    unique (user_id, token)
);

create table if not exists audit.audit_logs (
    audit_log_id uuid primary key default gen_random_uuid(),
    actor_user_id uuid null references auth.users(user_id),
    action varchar(80) not null,
    entity_type varchar(80) not null,
    entity_id varchar(120) null,
    payload jsonb null,
    occurred_at_utc timestamptz not null default now()
);

create table if not exists admin.admin_notes (
    admin_note_id uuid primary key default gen_random_uuid(),
    target_user_id uuid null references auth.users(user_id),
    note text not null,
    created_by_user_id uuid null references auth.users(user_id),
    created_at_utc timestamptz not null default now()
);

create table if not exists admin.system_settings (
    system_setting_id uuid primary key default gen_random_uuid(),
    setting_key varchar(120) not null unique,
    setting_value text not null,
    updated_by_user_id uuid null references auth.users(user_id),
    updated_at_utc timestamptz not null default now()
);

create table if not exists admin.moderation_actions (
    moderation_action_id uuid primary key default gen_random_uuid(),
    target_type varchar(80) not null,
    target_id varchar(120) not null,
    action_code varchar(60) not null,
    reason text null,
    acted_by_user_id uuid null references auth.users(user_id),
    acted_at_utc timestamptz not null default now()
);

create table if not exists integration.external_farmer_links (
    external_farmer_link_id uuid primary key default gen_random_uuid(),
    user_id uuid not null references auth.users(user_id),
    provider_code varchar(80) not null,
    external_id varchar(160) not null,
    created_at_utc timestamptz not null default now(),
    unique (provider_code, external_id)
);

create index if not exists idx_listings_seller on marketplace.listings (seller_user_id);
create index if not exists idx_listings_status on marketplace.listings (listing_status_id);
create index if not exists idx_enquiries_seller on messaging.enquiries (seller_user_id);
create index if not exists idx_enquiries_buyer on messaging.enquiries (buyer_user_id);
create index if not exists idx_notifications_user on messaging.notifications (user_id, created_at_utc desc);
