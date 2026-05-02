-- Read-Models for POS Projections
CREATE TABLE IF NOT EXISTS read_catalog_items (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    name TEXT NOT NULL,
    sku TEXT NOT NULL,
    price DECIMAL(18,2) NOT NULL,
    tax_rate DECIMAL(18,4) NOT NULL,
    track_inventory BOOLEAN NOT NULL,
    last_updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_read_catalog_sku ON read_catalog_items(sku);
CREATE INDEX IF NOT EXISTS idx_read_catalog_tenant ON read_catalog_items(tenant_id);

CREATE TABLE IF NOT EXISTS read_order_summaries (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    total_amount DECIMAL(18,2) NOT NULL,
    total_tax DECIMAL(18,2) NOT NULL,
    is_closed BOOLEAN NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS read_caja_balances (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    terminal_id TEXT NOT NULL,
    balance DECIMAL(18,2) NOT NULL,
    last_updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
