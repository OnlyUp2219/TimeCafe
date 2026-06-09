import {
    Accordion,
    AccordionHeader,
    AccordionItem,
    AccordionPanel,
    Badge,
    Button,
    Divider,
    Tag,
    Body1,
    Body2,
    Caption1,
    Subtitle1,
    Subtitle2Stronger,
    Title2,
    Title3,
    motionTokens,
} from "@fluentui/react-components";
import type { FC } from "react";
import { useMemo } from "react";
import { useNavigate } from "react-router-dom";
import { Header } from "@components/Header/Header";
import { Footer } from "@components/Footer/Footer";
import {
    Clock20Regular,
    MailCheckmark20Regular,
    Money20Regular,
    PersonAdd20Regular,
    ArrowRight20Regular,
} from "@fluentui/react-icons";
import { CURRENCY_SYMBOL } from "@shared/const/currency";
import { useComponentSize } from "@hooks/useComponentSize";

type FaqItem = {
    question: string;
    answer: string;
};

export const LandingPage: FC = () => {
    const navigate = useNavigate();
    const { sizes } = useComponentSize();

    const steps = useMemo(
        () => [
            {
                title: "1. Мгновенная авторизация",
                description: "Войдите за 10 секунд через Google или Microsoft без заполнения длинных регистрационных форм.",
            },
            {
                title: "2. Выбор тарифа и зоны",
                description: "Выберите поминутную оплату или почасовые стоп-чеки в зависимости от формата вашего отдыха или работы.",
            },
            {
                title: "3. Прозрачный контроль",
                description: "Отслеживайте время сессии и стоимость в реальном времени прямо со смартфона без очередей у ресепшена.",
            },
        ],
        []
    );

    const clientFeatures = useMemo(
        () => [
            {
                title: "Точность до секунды",
                description: "Никаких округлений на глаз. Наш биллинговый движок рассчитывает стоимость визита строго по времени пребывания.",
                tag: "Биллинг",
                icon: <Clock20Regular />,
                colSpan: "sm:col-span-2",
            },
            {
                title: "Бесконтактная оплата",
                description: "Пополняйте баланс банковской картой прямо в личном кабинете. Расчет в один клик без ожидания администратора.",
                tag: "Финансы",
                icon: <Money20Regular />,
                colSpan: "sm:col-span-1",
            },
            {
                title: "Единый профиль гостя",
                description: "Один аккаунт для всех заведений сети. Вход в один клик, сохранение персональных скидок и истории транзакций.",
                tag: "Профиль",
                icon: <PersonAdd20Regular />,
                colSpan: "sm:col-span-1",
            },
            {
                title: "Безопасность и аудит",
                description: "Все списания защищены транзакционной моделью, а действия персонала подлежат строгому логированию.",
                tag: "Безопасность",
                icon: <MailCheckmark20Regular />,
                colSpan: "sm:col-span-2",
            },
        ],
        []
    );

    const tariffCards = useMemo(
        () => [
            {
                title: "Поминутно",
                description: "Идеально для коротких встреч и спонтанных визитов. Оплата производится за каждую секунду проведенного времени.",
                highlight: "Гибко",
                price: `3 ${CURRENCY_SYMBOL} / мин`,
            },
            {
                title: "Почасово",
                description: "Выгоднее для длительной работы в коворкинге или долгих игровых сессий с друзьями.",
                highlight: "Популярно",
                price: `150 ${CURRENCY_SYMBOL} / час`,
            },
        ],
        []
    );

    const faq = useMemo<FaqItem[]>(
        () => [
            {
                question: "За что именно я плачу?",
                answer: "Вы оплачиваете только время нахождения в тайм-кафе или коворкинге. Чай, зерновой кофе, сладости, настольные игры, консоли и быстрый Wi-Fi всегда входят в стоимость.",
            },
            {
                question: "Как рассчитывается стоимость визита?",
                answer: "Биллинг TimeCafe автоматически применяет правила тарифа. Если в тарифе настроен стоп-чек (максимальная стоимость дня), система никогда не спишет больше этой суммы.",
            },
            {
                question: "Что произойдет, если баланс станет отрицательным?",
                answer: "При нехватке средств вы получите уведомление в кабинете. Пополнить баланс можно мгновенно банковской картой через Stripe прямо с вашего телефона.",
            },
        ],
        []
    );

    return (
        <main className="min-h-screen relative overflow-x-hidden flex flex-col" id="Landing Page">
            {/* Animated Ambient Background using Fluent UI Tokens */}
            <div className="absolute inset-0 pointer-events-none -z-10 overflow-hidden tc-ambient-bg">
                <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] rounded-full blur-[100px] opacity-30 tc-glow-brand animate-pulse-slow"></div>
                <div className="absolute bottom-[-10%] right-[-10%] w-[50%] h-[50%] rounded-full blur-[120px] opacity-20 tc-glow-accent animate-pulse-slow delay-1000"></div>
            </div>

            <Header variant="public" />

            <div className="flex-1 flex flex-col gap-24 py-16 mx-auto w-full max-w-[1400px] px-6 relative z-10">
                {/* Hero Section */}
                <section className="flex flex-col md:flex-row items-center gap-12 pt-8">
                    <div className="flex-1 flex flex-col gap-6 items-start">
                        <Tag appearance="brand" size="medium" className="rounded-full shadow-lg">Экосистема автоматизации</Tag>

                        <h1 className="text-5xl md:text-7xl font-bold tracking-tight text-transparent bg-clip-text tc-gradient-text pb-2">
                            Умный контроль времени.
                        </h1>

                        <Body1 className="text-lg md:text-xl max-w-lg opacity-90 leading-relaxed" style={{ color: 'var(--colorNeutralForeground2)' }}>
                            Исключите ошибки ручного учета, повысьте прозрачность расчетов и автоматизируйте запуск сессий гостей и бронирование ресурсов.
                        </Body1>

                        <div className="flex flex-wrap gap-4 mt-4">
                            <Button appearance="primary" size="large" onClick={() => navigate("/register")} icon={<ArrowRight20Regular />} iconPosition="after" className="shadow-xl">
                                Начать визит
                            </Button>
                            <Button appearance="subtle" size="large" onClick={() => navigate("/login")}>
                                Панель управления
                            </Button>
                        </div>
                    </div>

                    <div className="flex-1 w-full max-w-md relative perspective-1000">
                        {/* Floating Glass Card */}
                        <div className="tc-glass-panel p-6 border shadow-2xl transform rotate-y-[-10deg] rotate-x-[5deg] hover:rotate-0 transition-transform duration-700 ease-out">
                            <div className="flex items-center justify-between mb-6">
                                <div className="flex items-center gap-3">
                                    <div className="w-10 h-10 rounded-full flex items-center justify-center" style={{ backgroundColor: 'var(--colorBrandBackground2)' }}>
                                        <Clock20Regular style={{ color: 'var(--colorBrandForeground2)' }} />
                                    </div>
                                    <div>
                                        <Subtitle2Stronger block>Активный визит (Гость)</Subtitle2Stronger>
                                        <Caption1 style={{ color: 'var(--colorNeutralForeground3)' }}>Ресурс: VIP-зона (Стол №4)</Caption1>
                                    </div>
                                </div>
                                <Badge appearance="filled" color="success">В процессе</Badge>
                            </div>

                            <div className="flex justify-between items-end mb-4">
                                <div>
                                    <Caption1 block style={{ color: 'var(--colorNeutralForeground2)' }} className="mb-1">Время визита</Caption1>
                                    <div className="text-4xl font-light tabular-nums" style={{ color: 'var(--colorNeutralForeground1)' }}>1:42:05</div>
                                </div>
                                <div className="text-right">
                                    <Caption1 block style={{ color: 'var(--colorNeutralForeground2)' }} className="mb-1">Текущий счет</Caption1>
                                    <Title3 style={{ color: 'var(--colorBrandForeground1)' }}>155 {CURRENCY_SYMBOL}</Title3>
                                </div>
                            </div>

                            <Divider className="my-4" />
                            <div className="flex justify-between items-center">
                                <Body2 style={{ color: 'var(--colorNeutralForeground2)' }}>Тариф: Поминутный биллинг</Body2>
                                <Button appearance="transparent" size="small">Детали</Button>
                            </div>
                        </div>
                    </div>
                </section>

                {/* Steps Section */}
                <section className="flex flex-col gap-10">
                    <div className="text-center max-w-2xl mx-auto flex flex-col gap-3">
                        <Title2>Как это работает</Title2>
                        <Body1 style={{ color: 'var(--colorNeutralForeground2)' }}>Современный подход к учету времени гостей и управлению заведением.</Body1>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        {steps.map((s, idx) => (
                            <div key={s.title} className="tc-glass-panel p-8 border flex flex-col gap-4 hover:-translate-y-1 transition-transform duration-300">
                                <div className="w-12 h-12 rounded-xl flex items-center justify-center text-xl font-bold shadow-sm" style={{ backgroundColor: 'var(--colorBrandBackground)', color: 'var(--colorNeutralForegroundOnBrand)' }}>
                                    {idx + 1}
                                </div>
                                <Subtitle1>{s.title}</Subtitle1>
                                <Body1 style={{ color: 'var(--colorNeutralForeground2)' }}>{s.description}</Body1>
                            </div>
                        ))}
                    </div>
                </section>

                {/* Bento Grid Features */}
                <section className="flex flex-col gap-10">
                    <div className="flex flex-col gap-3">
                        <Title2>Эффективное управление</Title2>
                        <Body1 style={{ color: 'var(--colorNeutralForeground2)' }}>Технологические решения для гостей, администраторов и владельцев.</Body1>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 auto-rows-[200px]">
                        {clientFeatures.map((f) => (
                            <div key={f.title} className={`tc-glass-panel p-6 border flex flex-col justify-between group overflow-hidden relative ${f.colSpan}`}>
                                <div className="absolute top-0 right-0 w-32 h-32 rounded-full blur-[50px] opacity-0 group-hover:opacity-20 transition-opacity duration-500 tc-glow-brand"></div>

                                <div className="flex justify-between items-start z-10">
                                    <div className="w-12 h-12 rounded-xl flex items-center justify-center shadow-sm" style={{ backgroundColor: 'var(--colorNeutralBackground3)' }}>
                                        {f.icon}
                                    </div>
                                    <Badge appearance="outline">{f.tag}</Badge>
                                </div>
                                <div className="z-10">
                                    <Subtitle1 block className="mb-2">{f.title}</Subtitle1>
                                    <Body2 style={{ color: 'var(--colorNeutralForeground2)' }}>{f.description}</Body2>
                                </div>
                            </div>
                        ))}
                    </div>
                </section>

                {/* Tariffs Section */}
                <section className="flex flex-col gap-10">
                    <div className="text-center max-w-2xl mx-auto flex flex-col gap-3">
                        <Title2>Гибкая тарификация</Title2>
                        <Body1 style={{ color: 'var(--colorNeutralForeground2)' }}>Система поддерживает любые схемы: от посекундной оплаты до почасовых стоп-чеков.</Body1>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 max-w-4xl mx-auto w-full">
                        {tariffCards.map((t) => (
                            <div key={t.title} className="tc-glass-panel p-8 border flex flex-col gap-6 relative overflow-hidden group">
                                {t.highlight === "Популярно" && (
                                    <Tag
                                        appearance="brand"
                                        size="medium"
                                        className="absolute top-0 right-0 px-3 py-1 font-semibold rounded-bl-xl shadow-md z-20"
                                    >
                                        {t.highlight}
                                    </Tag>
                                )}
                                <div className="z-10">
                                    <Title3 block className="mb-2">{t.title}</Title3>
                                    <div className="text-3xl font-bold" style={{ color: 'var(--colorBrandForeground1)' }}>{t.price}</div>
                                </div>
                                <Body1 style={{ color: 'var(--colorNeutralForeground2)' }} className="flex-1 z-10">{t.description}</Body1>
                                <Button appearance={t.highlight === "Популярно" ? "primary" : "outline"} size="large" onClick={() => navigate("/register")} className="w-full mt-4 z-10">
                                    Выбрать
                                </Button>
                            </div>
                        ))}
                    </div>
                </section>

                {/* FAQ & CTA */}
                <section className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-start pb-10">
                    <div className="flex flex-col gap-6">
                        <div>
                            <Title2 block className="mb-3">Частые вопросы</Title2>
                            <Body1 style={{ color: 'var(--colorNeutralForeground2)' }}>Остались сомнения? Вот ответы на популярные вопросы.</Body1>
                        </div>
                        <div className="tc-glass-panel border p-2">
                            <Accordion collapsible>
                                {faq.map((item, idx) => (
                                    <AccordionItem key={item.question} value={idx}>
                                        <AccordionHeader as="h3" size={sizes.accordionHeader}>{item.question}</AccordionHeader>
                                        <AccordionPanel collapseMotion={{ duration: 250, easing: motionTokens.curveDecelerateMid } as any}>
                                            <div>
                                                <Body2 style={{ color: 'var(--colorNeutralForeground2)' }}>{item.answer}</Body2>
                                            </div>
                                        </AccordionPanel>
                                    </AccordionItem>
                                ))}
                            </Accordion>
                        </div>
                    </div>

                    <div className="tc-glass-panel p-10 border shadow-2xl relative overflow-hidden flex flex-col justify-center items-center text-center h-full min-h-[300px]">
                        <div className="absolute inset-0 opacity-10 tc-ambient-bg" style={{ background: 'linear-gradient(135deg, var(--colorBrandBackground), var(--colorNeutralBackground1))' }}></div>
                        <div className="relative z-10 flex flex-col items-center gap-6">
                            <Title2>Готовы автоматизировать ваше заведение?</Title2>
                            <Body1 style={{ color: 'var(--colorNeutralForeground2)' }} className="max-w-sm">
                                Создайте гостевой аккаунт для тестирования платформы или свяжитесь с нами для интеграции TimeCafe.
                            </Body1>
                            <Button appearance="primary" size="large" onClick={() => navigate("/register")} className="px-10 py-6 text-lg rounded-xl shadow-lg hover:scale-105 transition-transform">
                                Начать тест-драйв
                            </Button>
                        </div>
                    </div>
                </section>
            </div>

            <Footer />
        </main>
    );
};
