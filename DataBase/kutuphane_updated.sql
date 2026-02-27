-- Updated Database Schema for Library Management System v2.0
-- This script updates the existing database with new tables and columns

-- Use the database
USE kutuphane;

-- Update kitaplar table with new columns
ALTER TABLE kitaplar 
ADD COLUMN IF NOT EXISTS publisher VARCHAR(100) NULL AFTER ISBN,
ADD COLUMN IF NOT EXISTS publication_year INT NULL AFTER publisher,
ADD COLUMN IF NOT EXISTS category VARCHAR(50) NULL AFTER publication_year,
ADD COLUMN IF NOT EXISTS total_copies INT DEFAULT 1 AFTER category,
ADD COLUMN IF NOT EXISTS available_copies INT DEFAULT 1 AFTER total_copies,
ADD COLUMN IF NOT EXISTS shelf_location VARCHAR(50) NULL AFTER available_copies,
ADD COLUMN IF NOT EXISTS created_date DATETIME DEFAULT CURRENT_TIMESTAMP AFTER shelf_location,
ADD COLUMN IF NOT EXISTS modified_date DATETIME NULL AFTER created_date;

-- Modify ISBN column to VARCHAR for better compatibility
ALTER TABLE kitaplar MODIFY COLUMN ISBN VARCHAR(20) NULL;

-- Update login table for enhanced security
ALTER TABLE login 
ADD COLUMN IF NOT EXISTS ID INT AUTO_INCREMENT PRIMARY KEY FIRST,
ADD COLUMN IF NOT EXISTS password_hash VARCHAR(255) NULL AFTER password,
ADD COLUMN IF NOT EXISTS full_name VARCHAR(100) NULL AFTER password_hash,
ADD COLUMN IF NOT EXISTS email VARCHAR(100) NULL AFTER full_name,
ADD COLUMN IF NOT EXISTS phone VARCHAR(20) NULL AFTER email,
ADD COLUMN IF NOT EXISTS role INT DEFAULT 2 AFTER phone,
ADD COLUMN IF NOT EXISTS is_active BOOLEAN DEFAULT TRUE AFTER role,
ADD COLUMN IF NOT EXISTS created_date DATETIME DEFAULT CURRENT_TIMESTAMP AFTER is_active,
ADD COLUMN IF NOT EXISTS last_login_date DATETIME NULL AFTER created_date;

-- Create members table
CREATE TABLE IF NOT EXISTS members (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) NULL,
    phone VARCHAR(20) NULL,
    address TEXT NULL,
    membership_number VARCHAR(50) UNIQUE NOT NULL,
    membership_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    membership_expiry_date DATETIME NULL,
    is_active BOOLEAN DEFAULT TRUE,
    borrowed_books_count INT DEFAULT 0,
    total_fines DECIMAL(10,2) DEFAULT 0.00,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_membership_number (membership_number),
    INDEX idx_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create borrow_records table
CREATE TABLE IF NOT EXISTS borrow_records (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    book_id INT NOT NULL,
    member_id INT NOT NULL,
    borrow_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    due_date DATETIME NOT NULL,
    return_date DATETIME NULL,
    late_fee DECIMAL(10,2) NULL,
    status INT DEFAULT 1 COMMENT '1=Active, 2=Returned, 3=Overdue, 4=Lost',
    notes TEXT NULL,
    FOREIGN KEY (book_id) REFERENCES kitaplar(ID) ON DELETE RESTRICT,
    FOREIGN KEY (member_id) REFERENCES members(ID) ON DELETE RESTRICT,
    INDEX idx_book_id (book_id),
    INDEX idx_member_id (member_id),
    INDEX idx_status (status),
    INDEX idx_borrow_date (borrow_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create activity_log table for audit trail
CREATE TABLE IF NOT EXISTS activity_log (
    ID INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NULL,
    action_type VARCHAR(50) NOT NULL,
    description TEXT NOT NULL,
    ip_address VARCHAR(45) NULL,
    created_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES login(ID) ON DELETE SET NULL,
    INDEX idx_user_id (user_id),
    INDEX idx_action_type (action_type),
    INDEX idx_created_date (created_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert sample data for testing (if tables are empty)

-- Insert default admin user (password: admin123 - hashed)
INSERT IGNORE INTO login (username, password, password_hash, full_name, email, role, is_active)
VALUES ('admin', 'admin123', 'AQAAAAEAACcQAAAAEJ3qZ8vH5xKxJ0YvF5xKxJ0YvF5xKxJ0YvF5xKxJ0YvF5xKxJ0YvF5xKxJ0YvF5xKxJ0Yv==', 
        'System Administrator', 'admin@library.com', 1, TRUE);

-- Insert sample librarian user (password: librarian123 - hashed)
INSERT IGNORE INTO login (username, password, password_hash, full_name, email, role, is_active)
VALUES ('librarian', 'librarian123', 'AQAAAAEAACcQAAAAEJ3qZ8vH5xKxJ0YvF5xKxJ0YvF5xKxJ0YvF5xKxJ0YvF5xKxJ0YvF5xKxJ0YvF5xKxJ0Yv==', 
        'Library Staff', 'librarian@library.com', 2, TRUE);

-- Update existing books with default values
UPDATE kitaplar 
SET total_copies = 1, available_copies = 1, created_date = NOW()
WHERE total_copies IS NULL OR available_copies IS NULL;

-- Insert sample members
INSERT IGNORE INTO members (full_name, email, phone, membership_number, membership_date, membership_expiry_date)
VALUES 
('Ahmet Yılmaz', 'ahmet@example.com', '05551234567', 'M20260227001', NOW(), DATE_ADD(NOW(), INTERVAL 1 YEAR)),
('Ayşe Demir', 'ayse@example.com', '05559876543', 'M20260227002', NOW(), DATE_ADD(NOW(), INTERVAL 1 YEAR)),
('Mehmet Kaya', 'mehmet@example.com', '05556789012', 'M20260227003', NOW(), DATE_ADD(NOW(), INTERVAL 1 YEAR));

-- Insert sample book categories
UPDATE kitaplar SET category = 'Genel' WHERE category IS NULL;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_kitaplar_title ON kitaplar(ad);
CREATE INDEX IF NOT EXISTS idx_kitaplar_author ON kitaplar(yazar);
CREATE INDEX IF NOT EXISTS idx_kitaplar_isbn ON kitaplar(ISBN);
CREATE INDEX IF NOT EXISTS idx_kitaplar_category ON kitaplar(category);
CREATE INDEX IF NOT EXISTS idx_login_username ON login(username);

-- Create view for available books
CREATE OR REPLACE VIEW available_books AS
SELECT k.*, 
       (k.total_copies - k.available_copies) as borrowed_count
FROM kitaplar k
WHERE k.available_copies > 0;

-- Create view for overdue books
CREATE OR REPLACE VIEW overdue_books AS
SELECT b.ID, b.book_id, b.member_id, 
       k.ad as book_title, 
       m.full_name as member_name,
       b.borrow_date, b.due_date,
       DATEDIFF(NOW(), b.due_date) as days_overdue,
       (DATEDIFF(NOW(), b.due_date) * 2) as calculated_fee
FROM borrow_records b
INNER JOIN kitaplar k ON b.book_id = k.ID
INNER JOIN members m ON b.member_id = m.ID
WHERE b.status = 1 AND b.due_date < NOW();

-- Grant necessary permissions (adjust as needed)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON kutuphane.* TO 'library_user'@'localhost';

COMMIT;
