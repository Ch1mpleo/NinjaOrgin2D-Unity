# ?? LOGIN FLOW SETUP GUIDE
## H??ng d?n thi?t l?p lu?ng ??ng nh?p khi Start Game

---

## ?? CHECKLIST SETUP

### ? B??c 1: T?o AuthManager GameObject
1. Hierarchy ? **Create Empty**
2. ??i t�n: **`AuthManager`**
3. **Add Component** ? `Auth Manager` script
4. C?u h�nh trong Inspector:
   - ? **Create Test Account** = `true` (checked)
   - ? **Test Username** = `admin`
   - ? **Test Password** = `admin123`

### ? B??c 2: T?o AuthFlowManager GameObject
1. Hierarchy ? **Create Empty**
2. ??i t�n: **`AuthFlowManager`**
3. **Add Component** ? `Auth Flow Manager` script
4. C?u h�nh trong Inspector:
   ```
   Auth Flow Manager (Script)
   ?? UI
   ?  ?? Auth Panel: [K�o AuthPanel GameObject v�o ?�y]
   ?? Scene
      ?? Main Scene Name: [?? tr?ng n?u kh�ng c?n chuy?n scene]
   ```

### ? B??c 3: ?n Player l�c Start
1. T�m **Player** GameObject trong Hierarchy
2. **Uncheck** checkbox ? g�c tr�i (disable Player)
3. Player s? t? ??ng enable sau khi login th�nh c�ng

### ? B??c 4: ?n AuthPanel l�c Start
1. T�m **AuthPanel** GameObject trong Hierarchy
2. **Uncheck** checkbox ?? disable
3. AuthFlowManager s? t? ??ng show khi game start

---

## ?? C?U TR�C HIERARCHY CU?I C�NG

```
Hierarchy
?? Managers
?  ?? GameManager
?  ?? UIManager
?  ?? ... (c�c managers kh�c)
?? ?? AuthManager ? M?I (c� Auth Manager script)
?? ?? AuthFlowManager ? M?I (c� Auth Flow Manager script)
?  ?? Inspector: Auth Panel = AuthPanel
?? ?? Player ? DISABLED (uncheck ?? ?n)
?? Canvas
?  ?? ?? AuthPanel ? DISABLED (uncheck ?? ?n)
?  ?  ?? C� AuthPanel script
?  ?? Panel - Stats
?  ?? ... (UI kh�c)
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
?? Game b?t ??u! Player c� th? ch?i!
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
?? Quay v? m�n h�nh Login
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
   - Player xu?t hi?n v� c� th? ?i?u khi?n
   - Console log: `"AuthManager: User 'admin' logged in successfully."`

### **Test 2: Register New Account**
1. Click **Play** trong Unity
2. Nh?p Username/Password m?i
3. Click **Register**
4. ? K?t qu? mong ??i: `"Registration successful. Please login."`
5. Nh?p l?i th�ng tin v� Login
6. ? Player xu?t hi?n

### **Test 3: Login Fail**
1. Nh?p sai password
2. Click **Login**
3. ? K?t qu? mong ??i: Error message hi?n ra
4. Player v?n b? ?n

---

## ?? CONSOLE LOGS ?�NG

Khi ch?y game, Console n�n hi?n:

```
AuthManager: No users file found. Starting with empty user list.
AuthManager: Creating test account - Username: 'admin', Password: 'admin123'
AuthManager: Test account created successfully!
AuthManager: You can login with - Username: 'admin', Password: 'admin123'
```

Khi login th�nh c�ng:
```
AuthManager: User 'admin' logged in successfully.
AuthManager: Loaded profile for player 'test-player-id'.
AuthFlowManager: Enabled Player GameObject after login.
AuthFlowManager: Applied profile to Player successfully.
```

---

## ? TROUBLESHOOTING

### **V?n ?? 1: AuthPanel kh�ng hi?n**
- ? Ki?m tra: AuthFlowManager c� reference ??n AuthPanel ch?a?
- ? Ki?m tra: AuthPanel c� script AuthPanel ch?a?

### **V?n ?? 2: Player kh�ng enable sau login**
- ? Ki?m tra: Player GameObject c� b? disabled kh�ng?
- ? Ki?m tra: Player c� script Player.cs ch?a?
- ? Xem Console logs: "Enabled Player GameObject after login."

### **V?n ?? 3: Login th�nh c�ng nh?ng kh�ng load data**
- ? Ki?m tra: Player c� PlayerStats ScriptableObject ch?a?
- ? Xem Console: "Applied profile to Player successfully."

### **V?n ?? 4: AuthManager not found**
- ? ??m b?o: AuthManager GameObject t?n t?i trong scene
- ? ??m b?o: AuthManager c� script Auth Manager

---

## ?? OPTIONAL: Chuy?n Scene Sau Login

N?u mu?n load scene kh�c sau khi login:

### **C�ch 1: Load Scene Kh�c**
1. T?o scene m?i t�n `GameplayScene`
2. Trong AuthFlowManager Inspector:
   - **Main Scene Name** = `GameplayScene`
3. Scene ?� ph?i c�:
   - Player GameObject
   - Canvas v?i UI c?n thi?t

### **C�ch 2: Gi? Nguy�n Scene (Khuy�n d�ng)**
- ?? **Main Scene Name** = tr?ng
- Player s? ???c enable ngay trong scene hi?n t?i
- ??n gi?n h?n, �t l?i h?n

---

## ?? NOTES

1. **AuthManager v� AuthFlowManager** ??u c� `DontDestroyOnLoad`
   - Ch�ng s? t?n t?i qua nhi?u scene
   - Kh�ng b? destroy khi chuy?n scene

2. **Player Profile** ???c load/save t? ??ng
   - Khi login ? load profile
   - Khi logout ? save profile
   - Data ???c l?u b?ng SaveGame library

3. **Test Account** ch? d�ng cho development
   - Production n�n set `Create Test Account` = `false`

---

## ? DONE!

Sau khi setup xong, game c?a b?n s?:
1. ?? Hi?n Login Panel khi start
2. ? Y�u c?u login tr??c khi ch?i
3. ?? Enable Player sau khi login th�nh c�ng
4. ?? Load/Save player data t? ??ng
5. ?? Logout v� quay v? Login Panel

**Happy Coding! ??**
