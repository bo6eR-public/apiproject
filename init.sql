CREATE TABLE IF NOT EXISTS "products" (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO "products" (name, price, quantity) VALUES
    ('Ноутбук', 99999.99, 10),
    ('Смартфон', 49999.99, 25),
    ('Наушники', 7999.99, 50)
ON CONFLICT (id) DO NOTHING;
