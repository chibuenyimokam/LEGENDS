# LegendPay — Local Setup

`appsettings.json` is intentionally **not** committed (it holds API keys and passwords).
Each developer creates their own from the template.

## 1. Create your config

Copy the example file and rename the copy to `appsettings.json`:

```powershell
Copy-Item LegendPay\appsettings.Example.json LegendPay\appsettings.json
```

Then open `LegendPay\appsettings.json` and fill in the values:

| Key | What to put |
| --- | --- |
| `SendGrid:ApiKey` | The team SendGrid key (ask the team lead). Needed for OTP + admin 2FA emails. |
| `WalletStation:Username` / `Password` | The CoralPay sandbox credentials (ask the team lead). Needed for wallet provisioning. |
| `AdminSeed:Email` | **Your own email** — this is the admin account you log in with, and 2FA codes are sent here. |
| `AdminSeed:Password` | Any dev password you'll remember. |

## 2. Create the database

```powershell
dotnet ef database update --project LegendPay
```

## 3. Run

```powershell
dotnet run --project LegendPay --launch-profile https
```

The app runs in Development mode by default, which **auto-creates your admin account** from
`AdminSeed` on first run. Browse to `https://localhost:7036`.

## 4. Log into the admin portal

Go to `/Admin/Login` and use the `AdminSeed:Email` / `AdminSeed:Password` you set.

A 2FA code is emailed to that address. If your SendGrid key isn't set up yet, read the code
straight from your database instead:

```sql
SELECT TwoFactorCode FROM AdminAccounts WHERE Email = 'your-own-email@gmail.com';
```

> The admin account only seeds when `AdminSeed:Email` and `AdminSeed:Password` are both present
> in your config, and only if an admin with that email doesn't already exist.
