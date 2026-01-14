import {
    Accordion,
    AccordionHeader,
    AccordionItem,
    AccordionPanel,
    Badge,
    Button,
    Card,
    Divider,
    Tag,
    Text,
    Title2,
    tokens,
} from "@fluentui/react-components";
import type {FC} from "react";
import {useMemo} from "react";
import {useNavigate} from "react-router-dom";
import {Header} from "../components/Header/Header";
import {Footer} from "../components/Footer/Footer";
import {HoverTiltCard} from "../components/HoverTiltCard/HoverTiltCard";
import {TruncatedText} from "../components/TruncatedText/TruncatedText";
import {
    Clock20Regular,
    MailCheckmark20Regular,
    Money20Regular,
    PersonAdd20Regular,
} from "@fluentui/react-icons";

import blob1Url from "../assets/ssshape_blob1.svg";
import blob2Url from "../assets/ssshape_blob2.svg";
import squigglyUrl from "../assets/sssquiggly.svg";
import vortexUrl from "../assets/vvvortex.svg";

type FaqItem = {
    question: string;
    answer: string;
};

export const LandingPage: FC = () => {
    const navigate = useNavigate();

    const subtleTextStyle = useMemo(() => ({
        color: tokens.colorNeutralForeground2,
    }), []);

    const steps = useMemo(
        () => [
            {
                title: "Регистрируетесь",
                description: "Создайте аккаунт и подтвердите контакты",
            },
            {
                title: "Выбираете тариф",
                description: "Поминутно или почасово — как вам удобнее",
            },
            {
                title: "Контролируете визит",
                description: "Время и стоимость — прозрачно и в реальном времени",
            },
        ],
        []
    );

    const clientFeatures = useMemo(
        () => [
            {
                title: "Прозрачный расчёт",
                description: "Понимайте, за что платите, без сюрпризов",
                tag: "Визиты",
                icon: <Clock20Regular/>,
            },
            {
                title: "Баланс и операции",
                description: "Пополнение и история транзакций в одном месте",
                tag: "Финансы",
                icon: <Money20Regular/>,
            },
            {
                title: "Статусы профиля",
                description: "Черновик → Активный — всё понятно и предсказуемо",
                tag: "Профиль",
                icon: <PersonAdd20Regular/>,
            },
            {
                title: "Подтверждение контактов",
                description: "Безопасный вход и восстановление доступа",
                tag: "Безопасность",
                icon: <MailCheckmark20Regular/>,
            },
        ],
        []
    );

    const tariffCards = useMemo(
        () => [
            {
                title: "Поминутно",
                description: "Честная оплата за фактическое время",
                highlight: "Гибко",
            },
            {
                title: "Почасово",
                description: "Удобно для длительных визитов",
                highlight: "Просто",
            },
        ],
        []
    );

    const faq = useMemo<FaqItem[]>(
        () => [
            {
                question: "За что я плачу?",
                answer: "Вы платите за время, проведённое в заведении. Система показывает длительность визита и ориентировочную стоимость.",
            },
            {
                question: "Можно ли завершить визит раньше?",
                answer: "Да. Стоимость рассчитывается по выбранному тарифу и времени визита.",
            },
            {
                question: "Почему нужна верификация телефона?",
                answer: "Подтверждение телефона помогает защитить аккаунт и упрощает восстановление доступа, а также позволяет поддерживать связь по важным уведомлениям.",
            },
            {
                question: "Что если баланс станет недостаточным?",
                answer: "Вы сможете пополнить баланс. В будущем — обработка задолженности и удобные сценарии оплаты.",
            },
        ],
        []
    );

    return (
        <main
            className="tc-noise-overlay min-h-screen relative overflow-x-hidden"
            id="Landing Page"
        >
            <Header variant="public"/>

            <div className="pointer-events-none absolute inset-0 z-0 overflow-hidden">
                {/* top-right blob */}
                <img
                    src={blob1Url}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-20 -right-24 w-[360px] rotate-6 select-none sm:-top-24 sm:-right-28 sm:w-[640px]"
                    style={{opacity: 0.10}}
                    draggable={false}
                />

                {/* mid-left blob */}
                <img
                    src={blob2Url}
                    alt=""
                    aria-hidden="true"
                    className="absolute left-[-9rem] top-[34rem] w-[420px] -rotate-6 select-none sm:left-[-12rem] sm:top-[40rem] sm:w-[720px]"
                    style={{opacity: 0.10}}
                    draggable={false}
                />

                {/* waves near top */}
                <img
                    src={squigglyUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-20 left-1/2 w-[760px] -translate-x-1/2 select-none sm:-top-16 sm:w-[980px]"
                    style={{opacity: 0.10}}
                    draggable={false}
                />

                {/* waves near bottom */}
                <img
                    src={squigglyUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute bottom-[-10rem] left-1/2 w-[860px] -translate-x-1/2 rotate-180 select-none sm:bottom-[-14rem] sm:w-[1100px]"
                    style={{opacity: 0.10}}
                    draggable={false}
                />

                {/* vortex as accent around tariffs/faq */}
                <img
                    src={vortexUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute right-[-10rem] top-[64rem] w-[520px] select-none sm:right-[-8rem] sm:top-[72rem] sm:w-[720px]"
                    style={{opacity: 0.10}}
                    draggable={false}
                />
            </div>

            <div className="flex flex-col gap-10 py-10 z-10 mx-auto w-full max-w-6xl px-4 sm:px-6">
                <section className="flex flex-col gap-2">
                    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 sm:gap-10">
                        <div className="flex flex-col gap-4">
                            <Tag appearance="outline" className="w-fit">TimeCafe</Tag>

                            <Title2>
                                Плати только за время — без сюрпризов
                            </Title2>

                            <Text block style={subtleTextStyle}>
                                Запускайте визит, следите за таймером и прогнозом стоимости в реальном времени.
                            </Text>
                            <Text block style={subtleTextStyle}>
                                Баланс, история операций и статусы профиля — всегда под рукой.
                            </Text>

                            <div className="flex flex-col gap-3 sm:flex-row">
                                <Button appearance="primary" onClick={() => navigate("/register")}
                                        className="sm:w-auto">
                                    Зарегистрироваться
                                </Button>
                                <Button appearance="secondary" onClick={() => navigate("/login")}
                                        className="sm:w-auto">
                                    Войти
                                </Button>
                            </div>

                            <div className="flex flex-wrap gap-3">
                                <Badge appearance="tint" size="extra-large" shape="circular">Прозрачный расчёт</Badge>
                                <Badge appearance="tint" size="extra-large" shape="circular">Баланс</Badge>
                                <Badge appearance="tint" size="extra-large" shape="circular">Визиты</Badge>
                                <Badge appearance="tint" size="extra-large" shape="circular">Профиль</Badge>
                            </div>
                        </div>

                        <div className="hidden sm:block">
                            <div
                                className="rounded-2xl p-4"
                                style={{
                                    backgroundImage: `linear-gradient(180deg, ${tokens.colorBrandBackground2} 0%, ${tokens.colorNeutralBackground1} 100%)`,
                                    boxShadow: tokens.shadow4,
                                    border: `1px solid ${tokens.colorNeutralStroke1}`,
                                }}
                            >
                                <div className="grid grid-cols-1 gap-3">
                                    <HoverTiltCard className=" h-full">
                                        <div className="flex items-center justify-between">
                                            <Text weight="semibold">Баланс</Text>
                                            <Badge appearance="outline">Demo</Badge>
                                        </div>
                                        <Divider className="my-3"/>
                                        <Text size={500} weight="semibold">75 BYN</Text>
                                        <Text style={subtleTextStyle}>Доступно для оплаты визитов</Text>
                                    </HoverTiltCard>

                                    <HoverTiltCard className="h-full">
                                        <div className="flex items-center justify-between">
                                            <Text weight="semibold">Активный визит</Text>
                                            <Badge appearance="outline">Demo</Badge>
                                        </div>
                                        <Divider className="my-3"/>
                                        <div className="flex items-baseline justify-between">
                                            <Text size={500} weight="semibold">1:42</Text>
                                            <Text style={subtleTextStyle}>примерно 15.5 BYN</Text>
                                        </div>
                                        <Text style={subtleTextStyle}>Тариф: часовой</Text>
                                    </HoverTiltCard>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>

                <section className="flex flex-col gap-2">
                    <div className="flex flex-col gap-2">
                        <Text weight="semibold" block>Как это работает</Text>
                        <Text block style={subtleTextStyle}>Три шага — и вы в деле</Text>
                    </div>

                    <div className=" grid grid-cols-1 gap-3  sm:grid-cols-3">
                        {steps.map((s, idx) => (
                            <HoverTiltCard key={s.title} className="p-4 h-full">
                                <div className="flex items-start justify-between">
                                    <Text weight="semibold" block>{s.title}</Text>
                                    <Tag appearance="brand">{idx + 1}</Tag>
                                </div>
                                <TruncatedText textStyle={subtleTextStyle}>
                                    {s.description}
                                </TruncatedText>
                            </HoverTiltCard>
                        ))}
                    </div>
                </section>

                <section className="flex flex-col gap-2">
                    <div className="flex flex-col gap-2">
                        <Text weight="semibold" block>Возможности</Text>
                        <Text block style={subtleTextStyle}>Всё, что нужно гостю: визиты, баланс, профиль</Text>
                    </div>

                    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                        {clientFeatures.map((f) => (
                            <HoverTiltCard key={f.title} className="h-full">
                                <div className="flex items-start justify-between gap-4">
                                    <div className="flex min-w-0 items-start gap-3">
                                        <Badge
                                            appearance="tint" shape="rounded" size="extra-large"
                                        >
                                            {f.icon}
                                        </Badge>
                                        <div className="min-w-0 gap-2 flex flex-col">
                                            <Text weight="semibold" block>{f.title}</Text>
                                            <TruncatedText textStyle={subtleTextStyle}>
                                                {f.description}
                                            </TruncatedText>
                                        </div>
                                    </div>
                                    <Badge appearance="tint" size="large" shape="rounded"
                                           className="shrink-0">{f.tag}</Badge>
                                </div>
                            </HoverTiltCard>
                        ))}
                    </div>
                </section>

                <section className="flex flex-col gap-2">
                    <div className="flex flex-col gap-2">
                        <Text weight="semibold" block>Тарифы</Text>
                        <Text block style={subtleTextStyle}>Тариф выбирается перед визитом, оплата — по факту
                            времени</Text>
                    </div>

                    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                        {tariffCards.map((t) => (
                            <HoverTiltCard key={t.title} className="p-4 h-full">
                                <div className="flex items-center justify-between">
                                    <Text weight="semibold" block>{t.title}</Text>
                                    <Tag appearance="brand">{t.highlight}</Tag>
                                </div>
                                <TruncatedText className="mt-2" textStyle={subtleTextStyle}>
                                    {t.description}
                                </TruncatedText>
                                <div className="mt-4">
                                    <Button appearance="primary" onClick={() => navigate("/register")}
                                            className="w-full sm:w-auto">
                                        Начать
                                    </Button>
                                </div>
                            </HoverTiltCard>
                        ))}
                    </div>
                </section>

                <section className="flex flex-col gap-2">
                    <div className="flex flex-col gap-2">
                        <Text weight="semibold" block>FAQ</Text>
                        <Text block style={subtleTextStyle}>Короткие ответы на частые вопросы</Text>
                    </div>

                    <div>
                        <Card>
                            <Accordion collapsible defaultOpenItems={[0]}>
                                {faq.map((item, idx) => (
                                    <AccordionItem key={item.question} value={idx}>
                                        <AccordionHeader>{item.question}</AccordionHeader>
                                        <AccordionPanel>
                                            <Text block style={subtleTextStyle}>{item.answer}</Text>
                                        </AccordionPanel>
                                    </AccordionItem>
                                ))}
                            </Accordion>
                        </Card>
                    </div>

                    <div
                        className="mt-6 rounded-2xl p-4 sm:mt-8 sm:p-6"
                        style={{
                            backgroundColor: tokens.colorNeutralBackground1,
                            boxShadow: tokens.shadow4,
                            border: `1px solid ${tokens.colorNeutralStroke1}`,
                        }}
                    >
                        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                            <div>
                                <Text weight="semibold" block>Готовы попробовать?</Text>
                                <Text block style={subtleTextStyle}>Создайте аккаунт и начните первый визит</Text>
                            </div>
                            <div className="flex flex-col gap-2 sm:flex-row">
                                <Button appearance="primary" onClick={() => navigate("/register")}
                                        className="sm:w-auto">
                                    Регистрация
                                </Button>
                                <Button appearance="secondary" onClick={() => navigate("/login")}
                                        className="sm:w-auto">
                                    Войти
                                </Button>
                            </div>
                        </div>
                    </div>
                </section>
            </div>

            <Footer/>
        </main>
    );
};
