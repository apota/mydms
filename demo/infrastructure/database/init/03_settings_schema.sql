-- Settings Management Database Schema
-- This file initializes the settings management database

-- Create user_settings table
CREATE TABLE IF NOT EXISTS user_settings (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    settings JSONB NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(user_id)
);

-- Create system_settings table
CREATE TABLE IF NOT EXISTS system_settings (
    id SERIAL PRIMARY KEY,
    setting_key VARCHAR(255) NOT NULL UNIQUE,
    setting_value JSONB NOT NULL,
    description TEXT,
    is_public BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_user_settings_user_id ON user_settings(user_id);
CREATE INDEX IF NOT EXISTS idx_system_settings_key ON system_settings(setting_key);
CREATE INDEX IF NOT EXISTS idx_system_settings_public ON system_settings(is_public);

-- Insert default system settings
INSERT INTO system_settings (setting_key, setting_value, description, is_public) VALUES
    ('company_name', '"Demo Automotive"', 'Company name displayed in the application', true),
    ('company_logo', '""', 'URL to company logo', true),
    ('system_version', '"1.0.0"', 'Current system version', true),
    ('maintenance_mode', 'false', 'Enable/disable maintenance mode', false),
    ('max_upload_size', '10485760', 'Maximum file upload size in bytes (10MB)', false),
    ('session_timeout', '3600', 'Session timeout in seconds (1 hour)', false),
    ('password_policy', '{"minLength": 8, "requireUppercase": true, "requireLowercase": true, "requireNumbers": true, "requireSpecialChars": false}', 'Password policy configuration', false),
    ('email_notifications', 'true', 'Enable email notifications', false),
    ('backup_retention_days', '30', 'Number of days to retain backups', false),
    ('api_rate_limit', '1000', 'API rate limit per hour per user', false)
ON CONFLICT (setting_key) DO NOTHING;

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON TABLE user_settings TO dms_user;
GRANT ALL PRIVILEGES ON TABLE system_settings TO dms_user;
GRANT USAGE, SELECT ON SEQUENCE user_settings_id_seq TO dms_user;
GRANT USAGE, SELECT ON SEQUENCE system_settings_id_seq TO dms_user;
