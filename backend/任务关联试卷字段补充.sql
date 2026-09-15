-- ============================================
-- 任务关联试卷功能 - 数据库字段补充脚本
-- ============================================

-- 1. 为 learning_tasks 表添加 test_id 字段（关联测验）
ALTER TABLE learning_tasks 
ADD COLUMN test_id INT NULL COMMENT '关联的测验ID（可选，完成测验后自动关闭任务）' AFTER deadline;

-- 2. 为 learning_tasks 表添加 status 字段（任务状态）
ALTER TABLE learning_tasks 
ADD COLUMN status INT NOT NULL DEFAULT 0 COMMENT '任务状态：0-进行中，1-已完成，2-已关闭' AFTER test_id;

-- 3. 为已有任务设置默认状态
UPDATE learning_tasks SET status = 0 WHERE status IS NULL OR status = 0;

-- 4. 验证字段添加成功
SELECT id, task_name, target_org_id, deadline, test_id, status, created_at 
FROM learning_tasks 
LIMIT 10;

-- ============================================
-- 说明：
-- - test_id：可选字段，关联 exam_tests 表的测验ID
-- - status：任务状态，0=进行中，1=已完成，2=已关闭
-- - 当任务关联了测验（test_id不为空），且党员完成该测验后，
--   如果该党员已完成任务中的所有学习内容，任务状态自动变为2（已关闭）
-- ============================================
