namespace Auth.TimeCafe.Infrastructure.Services.Email;

public static class EmailTemplates
{
    private const string CoffeeColor = "#99634d";
    private const string LightCoffee = "#b8886f";
    private const string DarkCoffee = "#7a4f3d";
    private const string BackgroundColor = "#f5f5f5";
    private const string CardBackground = "#ffffff";
    private const string TextColor = "#323130";
    private const string SecondaryTextColor = "#605e5c";

    private static string GetBaseTemplate(string title, string content)
    {
        return $$"""

<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{{title}}</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, 'Roboto', 'Helvetica Neue', sans-serif;
            background-color: {{BackgroundColor}};
            padding: 20px;
            line-height: 1.6;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: {{CardBackground}};
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, {{CoffeeColor}} 0%, {{DarkCoffee}} 100%);
            padding: 40px 30px;
            text-align: center;
        }
        .header-icon {
            width: 64px;
            height: 64px;
            background-color: rgba(255, 255, 255, 0.2);
            border-radius: 50%;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            margin-bottom: 16px;
            font-size: 32px;
        }
        .header-title {
            color: #ffffff;
            font-size: 28px;
            font-weight: 600;
            margin: 0;
        }
        .content {
            padding: 40px 30px;
        }
        .content-text {
            color: {{TextColor}};
            font-size: 16px;
            margin-bottom: 24px;
        }
        .button {
            display: inline-block;
            padding: 14px 32px;
            background: linear-gradient(135deg, {{CoffeeColor}} 0%, {{LightCoffee}} 100%);
            color: #ffffff;
            text-decoration: none;
            border-radius: 6px;
            font-weight: 600;
            font-size: 16px;
            text-align: center;
            transition: transform 0.2s ease;
            box-shadow: 0 2px 4px rgba(153, 99, 77, 0.3);
        }
        .button:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 8px rgba(153, 99, 77, 0.4);
        }
        .code-box {
            background-color: {{BackgroundColor}};
            border-left: 4px solid {{CoffeeColor}};
            padding: 20px;
            border-radius: 6px;
            margin: 24px 0;
        }
        .code {
            font-family: 'Consolas', 'Monaco', 'Courier New', monospace;
            font-size: 32px;
            font-weight: 700;
            color: {{CoffeeColor}};
            letter-spacing: 4px;
            text-align: center;
            display: block;
        }
        .footer {
            background-color: {{BackgroundColor}};
            padding: 30px;
            text-align: center;
            border-top: 1px solid #edebe9;
        }
        .footer-text {
            color: {{SecondaryTextColor}};
            font-size: 14px;
            margin-bottom: 8px;
        }
        .divider {
            height: 1px;
            background-color: #edebe9;
            margin: 24px 0;
        }
        .info-box {
            background-color: #fef8f5;
            border: 1px solid #f0d9cc;
            border-radius: 6px;
            padding: 16px;
            margin-top: 24px;
        }
        .info-text {
            color: {{SecondaryTextColor}};
            font-size: 14px;
        }
        @media only screen and (max-width: 600px) {
            .container {
                margin: 0;
                border-radius: 0;
            }
            .header {
                padding: 30px 20px;
            }
            .content {
                padding: 30px 20px;
            }
            .footer {
                padding: 20px;
            }
        }
    </style>
</head>
<body>
    <div class="container">
        {{content}}
    </div>
</body>
</html>
""";
    }

    public static string GetConfirmationEmailTemplate(string confirmationLink)
    {
        var content = $"""

        <div class="header">
            <div class="header-icon">☕</div>
            <h1 class="header-title">TimeCafe</h1>
        </div>
        <div class="content">
            <p class="content-text">Здравствуйте!</p>
            <p class="content-text">
                Спасибо за регистрацию в <strong>TimeCafe</strong>! 
                Для завершения процесса регистрации, пожалуйста, подтвердите ваш email адрес.
            </p>
            <div style="text-align: center; margin: 32px 0;">
                <a href="{confirmationLink}" class="button">
                    ✓ Подтвердить Email
                </a>
            </div>
            <div class="divider"></div>
            <p class="content-text" style="font-size: 14px;">
                Если кнопка не работает, скопируйте и вставьте эту ссылку в ваш браузер:
            </p>
            <p style="color: {CoffeeColor}; font-size: 13px; word-break: break-all; margin-top: 8px;">
                {confirmationLink}
            </p>
            <div class="info-box">
                <p class="info-text">
                    <strong>Важно:</strong> Если вы не регистрировались в TimeCafe, просто проигнорируйте это письмо.
                </p>
            </div>
        </div>
        <div class="footer">
            <p class="footer-text">С уважением, команда TimeCafe</p>
            <p class="footer-text">© 2025 TimeCafe. Все права защищены.</p>
        </div>
""";

        return GetBaseTemplate("Подтверждение Email", content);
    }

    public static string GetPasswordResetLinkTemplate(string resetLink)
    {
        var content = $"""

        <div class="header">
            <div class="header-icon">🔐</div>
            <h1 class="header-title">Сброс пароля</h1>
        </div>
        <div class="content">
            <p class="content-text">Здравствуйте!</p>
            <p class="content-text">
                Мы получили запрос на сброс пароля для вашей учетной записи <strong>TimeCafe</strong>.
            </p>
            <p class="content-text">
                Нажмите на кнопку ниже, чтобы создать новый пароль:
            </p>
            <div style="text-align: center; margin: 32px 0;">
                <a href="{resetLink}" class="button">
                    🔑 Сбросить пароль
                </a>
            </div>
            <div class="divider"></div>
            <p class="content-text" style="font-size: 14px;">
                Если кнопка не работает, скопируйте и вставьте эту ссылку в ваш браузер:
            </p>
            <p style="color: {CoffeeColor}; font-size: 13px; word-break: break-all; margin-top: 8px;">
                {resetLink}
            </p>
            <div class="info-box">
                <p class="info-text">
                    <strong>Внимание:</strong> Если вы не запрашивали сброс пароля, проигнорируйте это письмо. 
                    Ваш пароль останется без изменений.
                </p>
            </div>
            <div class="info-box" style="margin-top: 16px;">
                <p class="info-text">
                    Эта ссылка действительна в течение ограниченного времени.
                </p>
            </div>
        </div>
        <div class="footer">
            <p class="footer-text">С уважением, команда TimeCafe</p>
            <p class="footer-text">© 2025 TimeCafe. Все права защищены.</p>
        </div>
""";

        return GetBaseTemplate("Сброс пароля", content);
    }

    public static string GetPasswordResetCodeTemplate(string resetCode)
    {
        var content = $"""

        <div class="header">
            <div class="header-icon">🔐</div>
            <h1 class="header-title">Код сброса пароля</h1>
        </div>
        <div class="content">
            <p class="content-text">Здравствуйте!</p>
            <p class="content-text">
                Мы получили запрос на сброс пароля для вашей учетной записи <strong>TimeCafe</strong>.
            </p>
            <p class="content-text">
                Используйте этот код для сброса пароля:
            </p>
            <div class="code-box">
                <span class="code">{resetCode}</span>
            </div>
            <p class="content-text" style="text-align: center; font-size: 14px; color: {SecondaryTextColor};">
                Введите этот код в форме сброса пароля
            </p>
            <div class="divider"></div>
            <div class="info-box">
                <p class="info-text">
                    <strong>Внимание:</strong> Если вы не запрашивали сброс пароля, проигнорируйте это письмо. 
                    Ваш пароль останется без изменений.
                </p>
            </div>
            <div class="info-box" style="margin-top: 16px;">
                <p class="info-text">
                    <strong>Важно:</strong> Этот код действителен в течение ограниченного времени. 
                    Никому не сообщайте этот код.
                </p>
            </div>
        </div>
        <div class="footer">
            <p class="footer-text">С уважением, команда TimeCafe</p>
            <p class="footer-text">© 2025 TimeCafe. Все права защищены.</p>
        </div>
""";

        return GetBaseTemplate("Код сброса пароля", content);
    }

}
