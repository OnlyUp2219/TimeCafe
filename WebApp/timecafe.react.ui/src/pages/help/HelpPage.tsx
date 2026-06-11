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
    Button,
    Divider,
    MessageBar,
    motionTokens
} from "@fluentui/react-components";
import { BookOpen20Regular, Code20Regular, Person20Regular, Settings20Regular, Money20Regular, Clock20Regular, Receipt20Regular, Key20Regular, DataUsage20Regular } from "@fluentui/react-icons";
import { usePermissions } from "@hooks/usePermissions";
import { AdminPanelPermission } from "@shared/auth/permissions";
import { useComponentSize } from "@hooks/useComponentSize";

export const HelpPage = () => {
    const { has } = usePermissions();
    const isAdmin = has(AdminPanelPermission);
    const { sizes } = useComponentSize();

    return (
        <div className="page-content flex flex-col gap-8 max-w-5xl mx-auto w-full pb-12">
            <div className="flex flex-col gap-4 text-center py-8">
                <LargeTitle>Центр поддержки и документации TimeCafe</LargeTitle>
                <Body2 className="text-(--colorNeutralForeground3) text-lg max-w-2xl mx-auto">
                    Добро пожаловать в исчерпывающее руководство по программному комплексу TimeCafe.
                    Здесь вы найдете ответы на любые вопросы: от базовой регистрации до глубокой настройки ролевой модели и API.
                </Body2>
            </div>

            {/* Секция 1: Общие сведения и Глоссарий */}
            <Card size={sizes.card} className="flex flex-col gap-6">
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
            <Card size={sizes.card} className="flex flex-col gap-6">
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
                    {/* Аутентификация */}
                    <AccordionItem value="auth">
                        <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Key20Regular />}>2.1 Регистрация и Безопасность</AccordionHeader>
                        <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
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
                    <AccordionItem value="billing">
                        <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Money20Regular />}>2.2 Управление Финансами и Балансом</AccordionHeader>
                        <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
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
                    <AccordionItem value="visits">
                        <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Clock20Regular />}>2.3 Жизненный цикл Визита</AccordionHeader>
                        <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                            <div className="flex flex-col gap-4">
                                <Body1>
                                    Открытие и закрытие визита — это ключевые операции в системе. Они могут инициироваться как вами через мобильное приложение, так и кассиром за стойкой.
                                </Body1>

                                <Title3>Запуск визита самостоятельно</Title3>
                                <ul className="list-disc ml-6 flex flex-col gap-2">
                                    <li>Перейдите в раздел <strong>Начать визит</strong>.</li>
                                    <li>Выберите <strong>Зону/Ресурс</strong>: У нас есть общие залы, VIP-комнаты, зоны PS5. Каждая зона имеет свою сетку тарифов.</li>
                                    <li>Выберите <strong>Тариф</strong>: Ознакомьтесь с условиями. Некоторые тарифы включают бесплатные напитки или требуют минимального времени пребывания.</li>
                                    <li>Укажите количество гостей, если вы пришли с друзьями и оплачиваете за всех.</li>
                                    <li>Нажмите кнопку <strong>Начать визит</strong>. После этого ваша заявка отправляется администратору на подтверждение. Как только статус сменится на "Подтвержден", ваше время начнется.</li>
                                </ul>
                            </div>
                        </AccordionPanel>
                    </AccordionItem>

                    {/* Округления и Стоп-чеки */}
                    <AccordionItem value="billing-rules">
                        <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<DataUsage20Regular />}>2.4 Правила округления и Стоп-чеки</AccordionHeader>
                        <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                            <div className="flex flex-col gap-4">
                                <Body1>
                                    Каждый тариф имеет собственные правила ценообразования, которые защищают вас от переплат и делают расчеты честными.
                                </Body1>

                                <ul className="list-disc ml-6 flex flex-col gap-2">
                                    <li><Body1Strong>Поминутные тарифы:</Body1Strong> Расчет происходит посекундно на основе точного времени пребывания в заведении.</li>
                                    <li><Body1Strong>Почасовые тарифы:</Body1Strong> Округляются до следующего полного часа в большую сторону (например, при сессии в 65 минут оплата будет списана за 2 часа).</li>
                                    <li><Body1Strong>Минимальное время визита:</Body1Strong> Гарантирует минимальное списание (например, 15 или 30 минут), даже если вы завершили визит сразу после входа.</li>
                                    <li><Body1Strong>Стоп-чек (Максимальный лимит):</Body1Strong> Ограничивает предельную стоимость визита за день. Например, если стоп-чек равен 600 руб., то сколько бы часов вы ни провели в заведении в рамках суток, итоговая стоимость не превысит 600 руб.</li>
                                </ul>
                            </div>
                        </AccordionPanel>
                    </AccordionItem>

                    {/* Скидки и Лояльность */}
                    <AccordionItem value="loyalty-rules">
                        <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Person20Regular />}>2.5 Программа лояльности и скидки</AccordionHeader>
                        <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                            <div className="flex flex-col gap-4">
                                <Body1>
                                    Система TimeCafe поощряет постоянных гостей автоматическим расчетом скидок.
                                </Body1>

                                <Title3>Правила применения скидок:</Title3>
                                <ul className="list-disc ml-6 flex flex-col gap-2">
                                    <li><Body1Strong>Глобальные акции:</Body1Strong> Действуют на все посещения в определенные часы или дни (например, утренняя скидка 20%).</li>
                                    <li><Body1Strong>Тарифные скидки:</Body1Strong> Распространяются на конкретные зоны или тарифы (например, скидка на VIP-комнату в будни).</li>
                                    <li><Body1Strong>Накопительная скидка гостя:</Body1Strong> Растет в зависимости от общей суммы, потраченной вами в сети заведений.</li>
                                </ul>

                                <MessageBar intent="info">
                                    Скидки в системе <strong>не суммируются</strong>. Биллинг-движок автоматически рассчитывает все доступные варианты и применяет одну, наиболее выгодную для гостя скидку.
                                </MessageBar>
                            </div>
                        </AccordionPanel>
                    </AccordionItem>

                    {/* Grace Period */}
                    <AccordionItem value="grace-rules">
                        <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Clock20Regular />}>2.6 Завершение визита и льготный период (Grace Period)</AccordionHeader>
                        <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                            <div className="flex flex-col gap-4">
                                <Body1>
                                    При завершении визита крайне важно зафиксировать точное время выхода, чтобы избежать лишних списаний.
                                </Body1>

                                <Title3>Как устроен выход:</Title3>
                                <ol className="list-decimal ml-6 flex flex-col gap-2">
                                    <li>Вы нажимаете кнопку <strong>Выход из заведения</strong> в личном кабинете.</li>
                                    <li>В этот момент создается фиксация времени и запускается <strong>3-минутный льготный период (Grace Period)</strong>.</li>
                                    <li>В течение льготного периода вы можете пополнить баланс картой онлайн через Stripe (без необходимости стоять в очереди).</li>
                                    <li>Как только оплата завершена или администратор на кассе подтвердил ваш расчет, визит закрывается по зафиксированному времени.</li>
                                </ol>

                                <MessageBar intent="warning">
                                    Если за 3 минуты оплата не была произведена, а администратор не подтвердил расчет на кассе, система считает, что вы продолжили визит. Льготный период аннулируется, таймер сессии возобновляет работу, и тарификация продолжается.
                                </MessageBar>
                            </div>
                        </AccordionPanel>
                    </AccordionItem>
                </Accordion>
            </Card>

            {/* Секция 3: Руководство администратора */}
            {isAdmin && (
                <Card size={sizes.card} className="flex flex-col gap-6">
                    <div className="flex items-center gap-3">
                        <Settings20Regular className="text-(--colorPaletteRedForeground1) text-3xl" />
                        <Title2>3. Панель Администратора (Секретный раздел)</Title2>
                    </div>

                    <Accordion collapsible multiple className="flex flex-col gap-2">

                        {/* Тарифы */}
                        <AccordionItem value="admin-tariffs">
                            <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Receipt20Regular />}>3.1 Управление Тарифами и Акциями</AccordionHeader>
                            <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
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
                        <AccordionItem value="admin-rbac">
                            <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Person20Regular />}>3.2 Роли, Доступы и Пользователи (DRBAC)</AccordionHeader>
                            <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
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

                        {/* Ресурсы */}
                        <AccordionItem value="admin-resources">
                            <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Settings20Regular />}>3.3 Управление ресурсами и зонами</AccordionHeader>
                            <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                                <div className="flex flex-col gap-4">
                                    <Body1>
                                        Ресурсы — это материальные объекты заведения, время использования которых подлежит учету и тарификации.
                                    </Body1>

                                    <Title3>Виды ресурсов:</Title3>
                                    <ul className="list-disc ml-6 flex flex-col gap-2">
                                        <li><Body1Strong>Рабочие места и столы:</Body1Strong> Стандартные посадочные места в общем зале или коворкинге.</li>
                                        <li><Body1Strong>Игровые зоны:</Body1Strong> Приставки PlayStation 5, VR-шлемы, требующие привязки к отдельным телевизорам/экранам.</li>
                                        <li><Body1Strong>Комнаты и залы:</Body1Strong> VIP-комнаты, переговорные, лектории, бронируемые под мероприятия.</li>
                                    </ul>

                                    <Title3>Контроль занятости:</Title3>
                                    <Body1>
                                        В панели администратора отображается интерактивная карта или список ресурсов. Активация визита гостя автоматически переводит привязанный ресурс в статус «Занят».
                                        Один ресурс не может быть занят двумя активными визитами одновременно (система выдаст ошибку валидации при попытке запуска).
                                    </Body1>
                                </div>
                            </AccordionPanel>
                        </AccordionItem>

                        {/* Walk-in */}
                        <AccordionItem value="admin-walkin">
                            <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Key20Regular />}>3.4 Регистрация анонимных гостей (Walk-in)</AccordionHeader>
                            <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                                <div className="flex flex-col gap-4">
                                    <Body1>
                                        Часто посетители заведения не имеют гостевого аккаунта в системе или пришли впервые без смартфона. Для них предусмотрен Walk-in сценарий.
                                    </Body1>

                                    <Title3>Шаги регистрации Walk-in гостя:</Title3>
                                    <ol className="list-decimal ml-6 flex flex-col gap-2">
                                        <li>Нажмите кнопку <strong>Быстрый визит (Walk-in)</strong> на панели администратора.</li>
                                        <li>Выберите свободный ресурс (номер стола или выданную пластиковую карту-номер).</li>
                                        <li>Выберите тариф.</li>
                                        <li>Система создаст временный анонимный визит без привязки к Email гостя. Расчет и оплата такого визита производятся администратором вручную при выходе гостя.</li>
                                    </ol>
                                </div>
                            </AccordionPanel>
                        </AccordionItem>

                        {/* Ведение смен */}
                        <AccordionItem value="admin-shifts">
                            <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<Receipt20Regular />}>3.5 Контроль кассы и закрытие визитов</AccordionHeader>
                            <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                                <div className="flex flex-col gap-4">
                                    <Body1>
                                        Администратор отвечает за проведение расчетов с гостями и сверку кассы в конце рабочего дня.
                                    </Body1>

                                    <Title3>Процедура расчета гостя:</Title3>
                                    <ol className="list-decimal ml-6 flex flex-col gap-2">
                                        <li>При обращении гостя найдите его активный визит в списке (или по номеру стола/карты).</li>
                                        <li>Нажмите <strong>Зафиксировать выход</strong>. Время остановится, и биллинг сформирует финальный счет с учетом скидок и доп. услуг.</li>
                                        <li>Примите оплату (наличные, банковский терминал или спишите средства с баланса, если у гостя достаточно денег).</li>
                                        <li>Отметьте счет как оплаченный. Визит перейдет в статус «Завершен», а ресурс освободится для новых гостей.</li>
                                    </ol>
                                </div>
                            </AccordionPanel>
                        </AccordionItem>

                        {/* Аудит */}
                        <AccordionItem value="admin-audit">
                            <AccordionHeader as="h3" size={sizes.accordionHeader} icon={<DataUsage20Regular />}>3.6 Журналы аудита и системные метрики</AccordionHeader>
                            <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                                <div className="flex flex-col gap-4">
                                    <Body1>
                                        Вся деятельность сотрудников фиксируется микросервисом Audit для предотвращения кассовых нарушений.
                                    </Body1>

                                    <Title3>Внутренний журнал аудита:</Title3>
                                    <Body1>
                                        В разделе <strong>Журнал аудита</strong> сохраняется история изменения тарифов, ролей и ручных корректировок баланса. Изменения сохраняются с детализацией до полей (старое/новое значение) и привязываются к учетной записи сотрудника.
                                    </Body1>

                                    <Title3>Grafana и Kibana:</Title3>
                                    <Body1>
                                        Системным инженерам доступны дашборды Grafana (нагрузка контейнеров, очереди RabbitMQ, время отклика API) и поисковый интерфейс Kibana для мониторинга ошибок по CorrelationId.
                                    </Body1>
                                </div>
                            </AccordionPanel>
                        </AccordionItem>

                    </Accordion>
                </Card>
            )}

            {/* Секция 4: Документация API для разработчиков */}
            <Card size={sizes.card} className="flex flex-col gap-6">
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
            <Card size={sizes.card} className="flex flex-col gap-4">
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
