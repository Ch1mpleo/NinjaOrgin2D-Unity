# ?? LOGIN FLOW SETUP GUIDE
## H??ng d?n thi?t l?p lu?ng ??ng nh?p khi Start Game

---

## ?? CHECKLIST SETUP

### ? B??c 1: T?o AuthManager GameObject
1. Hierarchy ? **Create Empty**
2. ??i tên: **`AuthManager`**
3. **Add Component** ? `Auth Manager` script
4. C?u hình trong Inspector:
   - ? **Create Test Account** = `true` (checked)
   - ? **Test Username** = `admin`
   - ? **Test Password** = `admin123`

### ? B??c 2: T?o AuthFlowManager GameObject
1. Hierarchy ? **Create Empty**
2. ??i tên: **`AuthFlowManager`**
3. **Add Component** ? `Auth Flow Manager` script
4. C?u hình trong Inspector:
   ```
   Auth Flow Manager (Script)
   ?? UI
   ?  ?? Auth Panel: [Kéo AuthPanel GameObject vào ?ây]
   ?? Scene
      ?? Main Scene Name: [?? tr?ng n?u không c?n chuy?n scene]
   ```

### ? B??c 3: ?n Player lúc Start
1. Tìm **Player** GameObject trong Hierarchy
2. **Uncheck** checkbox ? góc trái (disable Player)
3. Player s? t? ??ng enable sau khi login thành công

### ? B??c 4: ?n AuthPanel lúc Start
1. Tìm **AuthPanel** GameObject trong Hierarchy
2. **Uncheck** checkbox ?? disable
3. AuthFlowManager s? t? ??ng show khi game start

---

## ?? C?U TRÚC HIERARCHY CU?I CÙNG

```
Hierarchy
?? Managers
?  ?? GameManager
?  ?? UIManager
?  ?? ... (các managers khác)
?? ?? AuthManager ? M?I (có Auth Manager script)
?? ?? AuthFlowManager ? M?I (có Auth Flow Manager script)
?  ?? Inspector: Auth Panel = AuthPanel
?? ?? Player ? DISABLED (uncheck ?? ?n)
?? Canvas
?  ?? ?? AuthPanel ? DISABLED (uncheck ?? ?n)
?  ?  ?? Có AuthPanel script
?  ?? Panel - Stats
?  ?? ... (UI khác)
?? EventSystem
```

---

## ?? LU?NG HO?T ??NG

### **1. Game Start**
```
Unity Start
    ?
AuthManager.Awake() [DefaultExecutionOrder: -100]
    ? Load users & Create test account
AuthFlowManager.Awake() [DefaultExecutionOrder: -50]
    ?
AuthFlowManager.Start()
    ? Subscribe to AuthManager events
    ?
ShowLoginPanel() ? AuthPanel.Show()
```

### **2. User Login**
```
User nh?p Username/Password
    ? Click Login Button
AuthPanel ? OnLoginAttempt(username, password)
    ?
AuthFlowManager.OnLogin(username, password)
    ?
AuthManager.Login(username, password, out error)
    ? Verify credentials
AuthManager.OnUserLoggedIn?.Invoke(user) ?
```

### **3. Load Player**
```
AuthFlowManager.HandleUserLoggedIn(user)
    ? HideLoginPanel()
    ? LoadProfile(user.PlayerId)
    ?
ApplyProfileToExistingPlayer()
    ? Find Player GameObject
    ? player.gameObject.SetActive(true) ? ENABLE PLAYER
    ? player.ApplyProfile(profile) ? LOAD DATA
    ?
?? Game b?t ??u! Player có th? ch?i!
```

### **4. User Logout**
```
User click Logout Button
    ?
AuthManager.Logout()
    ? SaveCurrentPlayerProfile(user)
    ?
AuthManager.OnUserLoggedOut?.Invoke(user) ??
    ?
AuthFlowManager.HandleUserLoggedOut(user)
    ? ShowLoginPanel()
    ?
?? Quay v? màn hình Login
```

---

## ?? TEST GAME

### **Test 1: Login v?i Test Account**
1. Click **Play** trong Unity
2. AuthPanel s? hi?n ra
3. Nh?p:
   - **Username**: `admin`
   - **Password**: `admin123`
4. Click **Login**
5. ? K?t qu? mong ??i:
   - AuthPanel bi?n m?t
   - Player xu?t hi?n và có th? ?i?u khi?n
   - Console log: `"AuthManager: User 'admin' logged in successfully."`

### **Test 2: Register New Account**
1. Click **Play** trong Unity
2. Nh?p Username/Password m?i
3. Click **Register**
4. ? K?t qu? mong ??i: `"Registration successful. Please login."`
5. Nh?p l?i thông tin và Login
6. ? Player xu?t hi?n

### **Test 3: Login Fail**
1. Nh?p sai password
2. Click **Login**
3. ? K?t qu? mong ??i: Error message hi?n ra
4. Player v?n b? ?n

---

## ?? CONSOLE LOGS ?ÚNG

Khi ch?y game, Console nên hi?n:

```
AuthManager: No users file found. Starting with empty user list.
AuthManager: Creating test account - Username: 'admin', Password: 'admin123'
AuthManager: Test account created successfully!
AuthManager: You can login with - Username: 'admin', Password: 'admin123'
```

Khi login thành công:
```
AuthManager: User 'admin' logged in successfully.
AuthManager: Loaded profile for player 'test-player-id'.
AuthFlowManager: Enabled Player GameObject after login.
AuthFlowManager: Applied profile to Player successfully.
```

---

## ? TROUBLESHOOTING

### **V?n ?? 1: AuthPanel không hi?n**
- ? Ki?m tra: AuthFlowManager có reference ??n AuthPanel ch?a?
- ? Ki?m tra: AuthPanel có script AuthPanel ch?a?

### **V?n ?? 2: Player không enable sau login**
- ? Ki?m tra: Player GameObject có b? disabled không?
- ? Ki?m tra: Player có script Player.cs ch?a?
- ? Xem Console logs: "Enabled Player GameObject after login."

### **V?n ?? 3: Login thành công nh?ng không load data**
- ? Ki?m tra: Player có PlayerStats ScriptableObject ch?a?
- ? Xem Console: "Applied profile to Player successfully."

### **V?n ?? 4: AuthManager not found**
- ? ??m b?o: AuthManager GameObject t?n t?i trong scene
- ? ??m b?o: AuthManager có script Auth Manager

---

## ?? OPTIONAL: Chuy?n Scene Sau Login

N?u mu?n load scene khác sau khi login:

### **Cách 1: Load Scene Khác**
1. T?o scene m?i tên `GameplayScene`
2. Trong AuthFlowManager Inspector:
   - **Main Scene Name** = `GameplayScene`
3. Scene ?ó ph?i có:
   - Player GameObject
   - Canvas v?i UI c?n thi?t

### **Cách 2: Gi? Nguyên Scene (Khuyên dùng)**
- ?? **Main Scene Name** = tr?ng
- Player s? ???c enable ngay trong scene hi?n t?i
- ??n gi?n h?n, ít l?i h?n

---

## ?? NOTES

1. **AuthManager và AuthFlowManager** ??u có `DontDestroyOnLoad`
   - Chúng s? t?n t?i qua nhi?u scene
   - Không b? destroy khi chuy?n scene

2. **Player Profile** ???c load/save t? ??ng
   - Khi login ? load profile
   - Khi logout ? save profile
   - Data ???c l?u b?ng SaveGame library

3. **Test Account** ch? dùng cho development
   - Production nên set `Create Test Account` = `false`

---

## ? DONE!

Sau khi setup xong, game c?a b?n s?:
1. ?? Hi?n Login Panel khi start
2. ? Yêu c?u login tr??c khi ch?i
3. ?? Enable Player sau khi login thành công
4. ?? Load/Save player data t? ??ng
5. ?? Logout và quay v? Login Panel

**Happy Coding! ??**
