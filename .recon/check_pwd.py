# -*- coding: utf-8 -*-
# 只读：验证 partymembers 完整 bcrypt 密码哈希对应明文（不修改任何数据）
import bcrypt

hashes = [
    "$2a$10$lMKE7NG1O5nqK2QfUtvo5eyS1FN.Ow0akNbw2axTw4TZCMnHQO2Ny",
    "$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy",
]
candidates = ["123456", "12345678", "123456789", "88888888", "admin123", "password",
              "1234567890", "party123", "000000", "666666", "Aa123456", "admin123456",
              "123456a", "a123456", "1234567", "123456789", "test123", "admin",
              "zhangsan123", "123456qwe", "q123456", "Password123", "123123", "123321",
              "111111", "11111111", "abc123", "00000000"]

for h in hashes:
    found = []
    for pw in candidates:
        try:
            if bcrypt.checkpw(pw.encode("utf-8"), h.encode("utf-8")):
                found.append(pw)
        except Exception:
            pass
    print(h[:20], "=>", found)
