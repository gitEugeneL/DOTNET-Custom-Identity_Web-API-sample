-- =========================
-- 1. ENUM: role
-- =========================

DO
$$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'role') THEN
            CREATE TYPE role AS ENUM ('Admin', 'Manager', 'Customer');
        END IF;
    END
$$;

-- =========================
-- 2. FUNCTION: updated_at
-- =========================
CREATE OR REPLACE FUNCTION update_updated_at_column()
    RETURNS TRIGGER AS
$$
BEGIN
    IF NEW IS DISTINCT FROM OLD THEN
        NEW.updated_at := now();
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


-- =========================
-- 3. TABLE: users
-- =========================
CREATE TABLE IF NOT EXISTS users
(
    id                   UUID PRIMARY KEY     DEFAULT gen_random_uuid(),

    email                TEXT        NOT NULL UNIQUE,
    pwd_hash             BYTEA       NOT NULL,
    pwd_salt             BYTEA       NOT NULL,

    email_confirmed      BOOLEAN     NOT NULL DEFAULT FALSE,
    login_locked         BOOLEAN     NOT NULL DEFAULT FALSE,
    confirm_locked       BOOLEAN     NOT NULL DEFAULT FALSE,

    login_failed_count   INT         NOT NULL DEFAULT 0,
    confirm_failed_count INT         NOT NULL DEFAULT 0,
    generate_code_count  INT         NOT NULL DEFAULT 0,

    login_lock_expires   TIMESTAMPTZ,
    confirm_lock_expires TIMESTAMPTZ,

    role                 role        NOT NULL,

    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at           TIMESTAMPTZ
);


-- =========================
-- 4. TABLE: confirmation_codes
-- =========================
CREATE TABLE IF NOT EXISTS confirmation_codes
(
    id      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code    TEXT        NOT NULL,
    expires TIMESTAMPTZ NOT NULL,

    user_id UUID        NOT NULL REFERENCES users (id) ON DELETE CASCADE
);


-- =========================
-- 5. TABLE: refresh_tokens
-- =========================
CREATE TABLE IF NOT EXISTS refresh_tokens
(
    id      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    token   TEXT        NOT NULL,
    expires TIMESTAMPTZ NOT NULL,

    user_id UUID        NOT NULL REFERENCES users (id) ON DELETE CASCADE
);


-- =====================================================
-- INDEXES: USERS
-- =====================================================

-- auth / lookup
CREATE INDEX IF NOT EXISTS idx_users_role ON users (role);
CREATE INDEX IF NOT EXISTS idx_users_email_lower ON users (LOWER(email));

-- security / locking
CREATE INDEX IF NOT EXISTS idx_users_login_locked ON users (login_locked);
CREATE INDEX IF NOT EXISTS idx_users_confirm_locked ON users (confirm_locked);

-- cleanup / scheduled jobs
CREATE INDEX IF NOT EXISTS idx_users_login_lock_expires ON users (login_lock_expires);
CREATE INDEX IF NOT EXISTS idx_users_confirm_lock_expires ON users (confirm_lock_expires);

-- =====================================================
-- INDEXES: CONFIRMATION_CODES
-- =====================================================

CREATE INDEX IF NOT EXISTS idx_confirmation_codes_code ON confirmation_codes (code);
CREATE INDEX IF NOT EXISTS idx_confirmation_codes_expires ON confirmation_codes (expires);
CREATE UNIQUE INDEX IF NOT EXISTS idx_confirmation_codes_user_id_unique ON confirmation_codes (user_id);

-- =====================================================
-- INDEXES: REFRESH_TOKENS
-- =====================================================

CREATE UNIQUE INDEX IF NOT EXISTS idx_refresh_tokens_token ON refresh_tokens (token);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires ON refresh_tokens (expires);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON refresh_tokens (user_id);

-- =========================
-- TRIGGERS
-- =========================
DROP TRIGGER IF EXISTS trg_users_updated_at ON users;
CREATE TRIGGER trg_users_updated_at
    BEFORE UPDATE
    ON users
    FOR EACH ROW
EXECUTE FUNCTION update_updated_at_column();