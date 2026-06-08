import {
    Accordion,
    AccordionHeader,
    AccordionItem,
    AccordionPanel,
    Card,
    LargeTitle,
    Title3,
    Title2,
    Body1,
    Body1Strong,
    Body2,
    Link,
    Button,
    Divider,
    MessageBar,
    MessageBarBody,
    MessageBarTitle
} from "@fluentui/react-components";
import { BookOpen20Regular, Code20Regular, Person20Regular, Settings20Regular, QuestionCircle20Regular, LockClosed20Regular, Money20Regular, Clock20Regular, Receipt20Regular, Key20Regular, DataUsage20Regular } from "@fluentui/react-icons";
import { usePermissions } from "@hooks/usePermissions";
import { AdminPanelPermission } from "@shared/auth/permissions";

export const HelpPage = () => {
    const { has } = usePermissions();
    const isAdmin = has(AdminPanelPermission);

    return (
        <div className="page-content flex flex-col gap-8 max-w-5xl mx-auto w-full pb-12">
            <div className="flex flex-col gap-4 text-center py-8">
                <LargeTitle>Центр поддержки и документации TimeCafe</LargeTitle>
                <Body2 className="text-(--colorNeutralForeground3) text-lg max-w-2xl mx-auto">
                    Добро пожаловать в исчерпывающее руководство по программному комплексу TimeCafe.
                    Здесь вы найдете ответы на любые вопросы: от базовой регистрации до глубокой настройки ролевой модели и API.
                </Body2>
            </div>

            <MessageBar intent="info" layout="multiline">
                <MessageBarTitle>Важная информация</MessageBarTitle>
                <MessageBarBody>
                    Интерфейс данной справки динамически адаптируется под ваши права доступа. Если вы являетесь администратором,
                    вы увидите дополнительные скрытые разделы, недоступные обычным посетителям.
                </MessageBarBody>
            </MessageBar>

            {/* Секция 1: Общие сведения и Глоссарий */}
            <Card className="flex flex-col gap-6 p-8 shadow-md">
                <div className="flex items-center gap-3">
                    <BookOpen20Regular className="text-(--colorBrandForeground1) text-3xl" />
                    <Title2>1. Общая архитектура и концепция системы</Title2>
                </div>

                <div className="flex flex-col gap-4">
                    <Title3>1.1 Назначение системы</Title3>
                    <Body1>
                        Программный комплекс <strong>TimeCafe</strong> — это высоконагруженная распределенная микросервисная платформа,
                        разработанная специально для нужд антикафе, тайм-кафе, коворкингов и компьютерных клубов.
                        Основная задача системы — полная автоматизация процесса тарификации пребывания гостя, устранение
                        ошибок человеческого фактора при расчетах и повышение прозрачности биллинга.
                    </Body1>
                    <Body1>
                        В отличие от классических систем учета, TimeCafe использует концепцию <em>«предоплаченного или постоплаченного баланса»</em>,
                        где каждое действие пользователя фиксируется в журнале транзакций. Это позволяет внедрять гибкие системы лояльности,
                        персональные скидки и сложные правила округления времени.
                    </Body1>
                </div>

                <Divider />

                <div className="flex flex-col gap-4">
                    <Title3>1.2 Глоссарий терминов</Title3>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="flex flex-col gap-1 p-3 bg-(--colorNeutralBackground2) rounded-md">
                            <Body1Strong>Визит (Visit)</Body1Strong>
                            <Body2>Сеанс пребывания гостя в заведении. Имеет строгий жизненный цикл (Pending -&gt; Active -&gt; Completed/WaitingForPayment).</Body2>
                        </div>
                        <div className="flex flex-col gap-1 p-3 bg-(--colorNeutralBackground2) rounded-md">
                            <Body1Strong>Тариф (Tariff)</Body1Strong>
                            <Body2>Правило расчета стоимости. Может быть поминутным (PerMinute) или почасовым (Hourly) с настраиваемыми шагами округления.</Body2>
                        </div>
                        <div className="flex flex-col gap-1 p-3 bg-(--colorNeutralBackground2) rounded-md">
                            <Body1Strong>Ресурс (Resource)</Body1Strong>
                            <Body2>Физический или логический объект, который занимает гость (например, PlayStation 5, Стол №1, VIP-комната).</Body2>
                        </div>
                        <div className="flex flex-col gap-1 p-3 bg-(--colorNeutralBackground2) rounded-md">
                            <Body1Strong>Транзакция (Transaction)</Body1Strong>
                            <Body2>Неизменяемая запись о движении средств. Бывает трех типов: пополнение (Deposit), списание (Withdrawal) и корректировка (Adjustment).</Body2>
                        </div>
                    </div>
                </div>
            </Card>

            {/* Секция 2: Руководство посетителя */}
            <Card className="flex flex-col gap-6 p-8 shadow-md">
                <div className="flex items-center gap-3">
                    <Person20Regular className="text-(--colorBrandForeground1) text-3xl" />
                    <Title2>2. Полное руководство посетителя (Гостя)</Title2>
                </div>

                <Body1>
                    Данный раздел предназначен для посетителей тайм-кафе. Ознакомьтесь с подробными инструкциями по
                    использованию личного кабинета, управлению своим счетом и правилами тарификации.
                </Body1>

                <Accordion collapsible multiple className="flex flex-col gap-2">

                    {/* Аутентификация */}
                    <AccordionItem value="auth" className="bg-(--colorNeutralBackground1Hover) rounded-lg px-2">
                        <AccordionHeader size="large" icon={<Key20Regular />}>2.1 Регистрация и Безопасность</AccordionHeader>
                        <AccordionPanel className="pb-4">
                            <div className="flex flex-col gap-4">
                                <Body1>
                                    Безопасность ваших данных — наш приоритет. Процесс создания аккаунта защищен современными стандартами шифрования и
                                    многофакторной проверкой.
                                </Body1>

                                <Title3>Шаги для регистрации:</Title3>
                                <ul className="list-disc ml-6 flex flex-col gap-2">
                                    <li><Body1Strong>Заполнение формы:</Body1Strong> Перейдите на страницу регистрации. Введите корректный Email и надежный пароль. Пароль должен содержать минимум 8 символов, заглавные буквы и спецсимволы.</li>
                                    <li><Body1Strong>Подтверждение Email:</Body1Strong> На указанный вами адрес будет отправлено транзакционное письмо (через сервис Postmark). Перейдите по ссылке в письме в течение 24 часов. Без подтверждения почты функционал аккаунта будет ограничен.</li>
                                    <li><Body1Strong>Верификация телефона:</Body1Strong> В личном кабинете вы можете привязать номер телефона. На него придет SMS-код (через сервис Twilio). Это необходимо для получения статуса верифицированного гостя и доступа к премиальным тарифам.</li>
                                </ul>

                                <Title3>Социальная авторизация (OAuth 2.0)</Title3>
                                <Body1>
                                    Вы можете пропустить ручную регистрацию и войти в систему мгновенно, используя ваш аккаунт Google или Microsoft.
                                    Система автоматически создаст вам профиль и свяжет его с вашим Email. При этом вам не нужно будет запоминать отдельный пароль для TimeCafe.
                                </Body1>
                            </div>
                        </AccordionPanel>
                    </AccordionItem>

                    {/* Баланс */}
                    <AccordionItem value="billing" className="bg-(--colorNeutralBackground1Hover) rounded-lg px-2">
                        <AccordionHeader size="large" icon={<Money20Regular />}>2.2 Управление Финансами и Балансом</AccordionHeader>
                        <AccordionPanel className="pb-4">
                            <div className="flex flex-col gap-4">
                                <Body1>
                                    TimeCafe работает по принципу единого лицевого счета. Вы пополняете баланс любым удобным способом,
                                    а средства списываются автоматически по факту завершения вашего визита.
                                </Body1>

                                <Title3>Пополнение через Stripe (Онлайн оплата)</Title3>
                                <Body1>
                                    Для безналичного пополнения мы используем защищенный шлюз Stripe Checkout.
                                </Body1>
                                <ol className="list-decimal ml-6 flex flex-col gap-2">
                                    <li>Перейдите в раздел <strong>Баланс и транзакции</strong> в боковом меню.</li>
                                    <li>Нажмите кнопку <strong>Пополнить баланс</strong>.</li>
                                    <li>Выберите одну из предустановленных сумм (например, 500, 1000, 2000 руб.) или введите произвольную сумму.</li>
                                    <li>Вы будете перенаправлены на страницу Stripe. Вы можете использовать банковские карты (Visa, Mastercard), Apple Pay или Google Pay.</li>
                                    <li>После успешной транзакции Stripe отправит вебхук в наш биллинг-сервис, и ваш баланс обновится моментально. В случае задержек обновите страницу.</li>
                                </ol>

                                <MessageBar intent="warning">
                                    Если после оплаты баланс не изменился в течение 5 минут, обратитесь к администратору. Ваша транзакция сохранена в базе данных Stripe и никуда не пропадет.
                                </MessageBar>

                                <Title3>Что такое "Задолженность" (Debt)?</Title3>
                                <Body1>
                                    Если во время вашего пребывания в заведении ваши расходы превысили остаток на балансе, система зафиксирует отрицательный остаток как Задолженность.
                                    Вы не сможете начать новый визит до полного погашения долга. При следующем пополнении баланса система в первую очередь спишет сумму в счет погашения задолженности.
                                </Body1>
                            </div>
                        </AccordionPanel>
                    </AccordionItem>

                    {/* Визиты */}
                    <AccordionItem value="visits" className="bg-(--colorNeutralBackground1Hover) rounded-lg px-2">
                        <AccordionHeader size="large" icon={<Clock20Regular />}>2.3 Жизненный цикл Визита</AccordionHeader>
                        <AccordionPanel className="pb-4">
                            <div className="flex flex-col gap-4">
                                <Body1>
                                    Открытие и закрытие визита — это ключевые операции в системе. Они могут инициироваться как вами через мобильное приложение, так и кассиром за стойкой.
                                </Body1>

                                <Title3>Запуск визита самостоятельно</Title3>
                                <ul className="list-disc ml-6 flex flex-col gap-2">
                                    <li>Перейдите в раздел <strong>Начать визит</strong>.</li>
                                    <li>Выберите <strong>Зону/Ресурс</strong>: У нас есть общие залы, VIP-комнаты, зоны PS5. Каждая зона имеет свою сетку тарифов.</li>
                                    <li>Выберите <strong>Тариф</strong>: Ознакомьтесь с условиями. Некоторые тарифы включают бесплатные напитки или требуют минимального времени пребывания (стоп-чек).</li>
                                    <li>Укажите количество гостей, если вы пришли с друзьями и оплачиваете за всех.</li>
                                    <li>Нажмите кнопку <strong>Начать визит</strong>.</li>
                                </ul>

                                <Title3>Как работает расчет стоимости?</Title3>
                                <Body1>Расчет (Billing Engine) происходит строго в момент <strong>закрытия визита</strong>.</Body1>
                                <div className="bg-(--colorNeutralBackground3) p-4 rounded-md font-mono text-sm">
                                    Формула: ( Базовая стоимость * Время (с учетом округления) ) - Скидки + Доп. услуги
                                </div>
                                <Body1>
                                    <strong>Пример округления:</strong> Если тариф предполагает округление до 15 минут, а вы пробыли 16 минут,
                                    система посчитает ваше время как 30 минут. Всегда уточняйте правила округления в описании тарифа!
                                </Body1>
                            </div>
                        </AccordionPanel>
                    </AccordionItem>
                </Accordion>
            </Card>

            {/* Секция 3: Руководство администратора */}
            {isAdmin && (
                <Card className="flex flex-col gap-6 p-8 shadow-md border-2 border-(--colorPaletteRedBorder2)">
                    <div className="flex items-center gap-3">
                        <Settings20Regular className="text-(--colorPaletteRedForeground1) text-3xl" />
                        <Title2>3. Панель Администратора (Секретный раздел)</Title2>
                    </div>

                    <MessageBar intent="error" layout="multiline">
                        <MessageBarTitle>Конфиденциально</MessageBarTitle>
                        <MessageBarBody>
                            Этот раздел доступен только сотрудникам с правами <strong>AdminPanelPermission</strong>.
                            Все ваши действия логируются микросервисом Audit. Будьте осторожны при редактировании тарифов и балансов пользователей.
                        </MessageBarBody>
                    </MessageBar>

                    <Accordion collapsible multiple className="flex flex-col gap-2">

                        {/* Тарифы */}
                        <AccordionItem value="admin-tariffs" className="bg-(--colorNeutralBackground1Hover) rounded-lg px-2">
                            <AccordionHeader size="large" icon={<Receipt20Regular />}>3.1 Управление Тарифами и Акциями</AccordionHeader>
                            <AccordionPanel className="pb-4">
                                <div className="flex flex-col gap-4">
                                    <Body1>
                                        Система тарификации (Venue Service) поддерживает сложную бизнес-логику ценообразования.
                                    </Body1>

                                    <Title3>Создание тарифа</Title3>
                                    <ul className="list-disc ml-6 flex flex-col gap-2">
                                        <li><Body1Strong>PerMinute (Поминутный):</Body1Strong> Укажите стоимость одной минуты. Гость платит ровно за то время, которое пробыл.</li>
                                        <li><Body1Strong>Hourly (Почасовой):</Body1Strong> Укажите стоимость часа. Дополнительно настраивается параметр <em>Rounding Rule</em>. Например, если правило "Round to 60", то любая начатая минута следующего часа считается как полный час.</li>
                                        <li><Body1Strong>Визуальные темы (Themes):</Body1Strong> Вы можете привязать к тарифу цветную тему (Hex-коды), чтобы карточки тарифов красиво отображались на витрине гостя.</li>
                                    </ul>

                                    <Title3>Система скидок (Promotions)</Title3>
                                    <Body1>
                                        Скидки могут применяться глобально (ко всем тарифам) или локально (к конкретному тарифу).
                                        Они имеют срок действия (ValidFrom / ValidTo). Если у гостя есть персональная скидка из профиля,
                                        система выберет <strong>максимальную</strong> из доступных скидок (они не суммируются, чтобы избежать убытков заведения).
                                    </Body1>
                                </div>
                            </AccordionPanel>
                        </AccordionItem>

                        {/* Пользователи и RBAC */}
                        <AccordionItem value="admin-rbac" className="bg-(--colorNeutralBackground1Hover) rounded-lg px-2">
                            <AccordionHeader size="large" icon={<Person20Regular />}>3.2 Роли, Доступы и Пользователи (DRBAC)</AccordionHeader>
                            <AccordionPanel className="pb-4">
                                <div className="flex flex-col gap-4">
                                    <Body1>
                                        Сервис авторизации реализует Dynamic Role-Based Access Control. Это означает, что права не «зашиты» в код,
                                        а могут настраиваться прямо из интерфейса.
                                    </Body1>

                                    <Title3>Управление ролями (Roles & Claims)</Title3>
                                    <Body1>
                                        В разделе <strong>Роли и разрешения</strong> вы можете создавать новые должности (например, «Старший кассир»)
                                        и назначать им гранулярные разрешения. Разрешения определяют, может ли сотрудник:
                                    </Body1>
                                    <ul className="list-disc ml-6 flex flex-col gap-2">
                                        <li>Открывать и закрывать смены (ManageVisits).</li>
                                        <li>Корректировать баланс пользователей (ManageBilling).</li>
                                        <li>Просматривать аудит-логи (ViewAuditLogs).</li>
                                        <li>Редактировать конфигурацию тарифов (ManageTariffs).</li>
                                    </ul>

                                    <Title3>Ручная корректировка баланса</Title3>
                                    <Body1>
                                        Если гость оплатил наличными в кассу, вы обязаны зачислить эту сумму на его виртуальный счет в TimeCafe. 
                                        Зайдите в карточку пользователя &rarr; Баланс &rarr; <strong>Корректировка</strong>. Обязательно укажите причину в поле Reason (например, "Оплата наличными, чек №123").
                                        Эта операция создаст транзакцию типа <em>Adjustment</em>.
                                    </Body1>
                                </div>
                            </AccordionPanel>
                        </AccordionItem>

                        {/* Аудит */}
                        <AccordionItem value="admin-audit" className="bg-(--colorNeutralBackground1Hover) rounded-lg px-2">
                            <AccordionHeader size="large" icon={<DataUsage20Regular />}>3.3 Аудит и Мониторинг (Kibana & Grafana)</AccordionHeader>
                            <AccordionPanel className="pb-4">
                                <div className="flex flex-col gap-4">
                                    <Body1>
                                        Платформа TimeCafe построена с фокусом на наблюдаемость (Observability). Администраторам доступны инструменты уровня Enterprise.
                                    </Body1>

                                    <Title3>Внутренний журнал аудита (Audit Service)</Title3>
                                    <Body1>
                                        В разделе <strong>Журнал аудита</strong> хранится история всех изменений критичных сущностей.
                                        Система сохраняет старое состояние (OldData) и новое состояние (NewData) в формате JSON. Вы можете легко восстановить хронологию событий, если возник конфликт с гостем.
                                    </Body1>

                                    <Title3>Grafana (Метрики и производительность)</Title3>
                                    <Body1>
                                        На вкладке Grafana отображаются системные метрики: использование CPU/RAM контейнерами Docker, задержки (latency) API запросов,
                                        размер очередей в RabbitMQ. Это позволяет системному администратору вовремя заметить перегрузку сервера.
                                    </Body1>

                                    <Title3>Kibana (Глубокий анализ логов)</Title3>
                                    <Body1>
                                        Elasticsearch собирает текстовые логи со всех микросервисов. Через Kibana вы можете искать ошибки (Exception) по CorrelationId.
                                        Если пользователь сообщает о проблеме в 14:05, вы можете отфильтровать логи именно за эту минуту и найти причину.
                                    </Body1>
                                </div>
                            </AccordionPanel>
                        </AccordionItem>

                    </Accordion>
                </Card>
            )}

            {/* Секция 4: Документация API для разработчиков */}
            <Card className="flex flex-col gap-6 p-8 shadow-md bg-(--colorNeutralBackground3)">
                <div className="flex items-center gap-3">
                    <Code20Regular className="text-(--colorBrandForeground1) text-3xl" />
                    <Title2>4. API для Разработчиков (OpenAPI / Scalar)</Title2>
                </div>

                <div className="flex flex-col gap-4">
                    <Body1>
                        Архитектура TimeCafe предполагает возможность легкой интеграции со сторонними системами (например, вашими внутренними ERP,
                        ботами в Telegram или мобильными приложениями). Для этого мы предоставляем полнофункциональное REST API.
                    </Body1>

                    <Title3>Особенности нашего API</Title3>
                    <ul className="list-disc ml-6 flex flex-col gap-2">
                        <li><Body1Strong>Microservices Gateway:</Body1Strong> Вам не нужно обращаться к каждому сервису по отдельности. Единая точка входа — шлюз YARP Proxy (порт 8010).</li>
                        <li><Body1Strong>Авторизация:</Body1Strong> Используется Bearer JWT Token. Для межсервисного взаимодействия применяются gRPC вызовы.</li>
                        <li><Body1Strong>Идемпотентность:</Body1Strong> Финансовые операции (например, списания) защищены ключами идемпотентности, что исключает двойные списания при сетевых сбоях.</li>
                        <li><Body1Strong>Result Pattern:</Body1Strong> API не возвращает неожиданных HTTP 500. Все бизнес-ошибки инкапсулированы в стандартизированный JSON-ответ с перечнем кодов ошибок.</li>
                    </ul>

                    <Body1>
                        Вместо устаревшего Swagger UI мы используем современный инструмент <strong>Scalar API Reference</strong>.
                        Он позволяет не только читать документацию, но и содержит встроенного HTTP-клиента для тестирования запросов
                        и автоматической генерации кода на 20+ языках программирования (Python, Go, Java, cURL и др.).
                    </Body1>

                    <div className="mt-4">
                        <Button
                            appearance="primary"
                            size="large"
                            icon={<Code20Regular />}
                            as="a"
                            href="/scalar/v1"
                            target="_blank"
                        >
                            Запустить интерактивную песочницу Scalar API
                        </Button>
                    </div>
                </div>
            </Card>

            {/* Секция 5: Поддержка */}
            <Card className="flex flex-col gap-4 p-8 shadow-md border-t-4 border-(--colorBrandStroke1)">
                <Title2>Не нашли ответ на свой вопрос?</Title2>
                <Body1>
                    Если вы столкнулись с системной ошибкой, багом при оплате или вам требуется консультация по сложной настройке тарифов,
                    наша служба технической поддержки всегда на связи.
                </Body1>
                <div className="flex gap-4 mt-2">
                    <Button appearance="secondary" as="a" href="mailto:support@timecafe.example.com">Написать в Support (Email)</Button>
                    <Button appearance="outline" as="a" href="https://t.me/TimeCafeSupportBot" target="_blank">Чат в Telegram</Button>
                </div>
                <Body2 className="text-(--colorNeutralForeground3) mt-4">
                    График работы технической поддержки: Круглосуточно 24/7 для критических инцидентов (Severity 1).
                    Для обычных вопросов время ответа составляет до 4 часов.
                </Body2>
            </Card>

        </div>
    );
};
