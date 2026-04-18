insert into auth.roles (role_code, role_name)
values
    ('SELLER', 'Seller'),
    ('BUYER', 'Buyer'),
    ('ADMIN', 'Administrator')
on conflict (role_code) do nothing;

insert into catalog.listing_statuses (status_code, status_name)
values
    ('DRAFT', 'Draft'),
    ('PUBLISHED', 'Published'),
    ('UNPUBLISHED', 'Unpublished'),
    ('SOLD_OUT', 'Sold Out'),
    ('ARCHIVED', 'Archived'),
    ('EXPIRED', 'Expired')
on conflict (status_code) do nothing;

insert into catalog.subscription_statuses (status_code, status_name)
values
    ('ACTIVE', 'Active'),
    ('EXPIRED', 'Expired'),
    ('CANCELLED', 'Cancelled')
on conflict (status_code) do nothing;

insert into catalog.payment_statuses (status_code, status_name)
values
    ('PENDING', 'Pending'),
    ('SUCCESS', 'Success'),
    ('FAILED', 'Failed')
on conflict (status_code) do nothing;

insert into catalog.enquiry_statuses (status_code, status_name)
values
    ('NEW', 'New'),
    ('IN_PROGRESS', 'In Progress'),
    ('CLOSED', 'Closed')
on conflict (status_code) do nothing;

insert into catalog.contact_modes (mode_code, mode_name)
values
    ('PHONE', 'Phone Call'),
    ('SMS', 'SMS'),
    ('APP_CHAT', 'In-App Chat')
on conflict (mode_code) do nothing;

insert into catalog.payment_methods (method_code, method_name)
values
    ('CARD', 'Card'),
    ('BANK_TRANSFER', 'Bank Transfer'),
    ('MOBILE_MONEY', 'Mobile Money')
on conflict (method_code) do nothing;

insert into catalog.units (unit_code, unit_name)
values
    ('KG', 'Kilogram'),
    ('TON', 'Ton'),
    ('BAG', 'Bag'),
    ('HEAD', 'Head')
on conflict (unit_code) do nothing;

insert into catalog.categories (category_code, category_name)
values
    ('CROPS', 'Crops'),
    ('LIVESTOCK', 'Livestock')
on conflict (category_code) do nothing;

insert into catalog.product_types (category_id, product_type_name)
select c.category_id, t.product_type_name
from (
    values
        ('CROPS', 'Maize'),
        ('CROPS', 'Beans'),
        ('CROPS', 'Rice'),
        ('CROPS', 'Tomatoes'),
        ('LIVESTOCK', 'Cattle'),
        ('LIVESTOCK', 'Goats'),
        ('LIVESTOCK', 'Poultry'),
        ('LIVESTOCK', 'Pigs')
) as t(category_code, product_type_name)
inner join catalog.categories c on c.category_code = t.category_code
where not exists (
    select 1
    from catalog.product_types pt
    where pt.category_id = c.category_id
      and lower(pt.product_type_name) = lower(t.product_type_name)
);

insert into billing.subscription_plans (plan_name, price_amount, duration_days, max_active_listings, is_active, sort_order)
values
    ('Starter', 9.99, 30, 10, true, 1),
    ('Growth', 24.99, 30, 50, true, 2),
    ('Pro', 59.99, 30, 200, true, 3)
on conflict do nothing;
