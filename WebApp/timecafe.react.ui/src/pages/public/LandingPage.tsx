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
    Body1,
    Body2,
    Caption1,
    Subtitle1,
    Subtitle2Stronger,
    Title2,
    Title3,
    tokens,
} from "@fluentui/react-components";
import type {FC} from "react";
import {useMemo} from "react";
import {useNavigate} from "react-router-dom";
import {Header} from "@components/Header/Header";
import {Footer} from "@components/Footer/Footer";
import {HoverTiltCard} from "@components/HoverTiltCard/HoverTiltCard";
import {
    Clock20Regular,
    MailCheckmark20Regular,
    Money20Regular,
    PersonAdd20Regular,
} from "@fluentui/react-icons";

import blob1Url from "@assets/ssshape_blob1.svg";
import blob2Url from "@assets/ssshape_blob2.svg";
import squigglyUrl from "@assets/sssquiggly.svg";
import vortexUrl from "@assets/vvvortex.svg";

type FaqItem = {
    question: string;
    answer: string;
};

export const LandingPage: FC = () => {
    const navigate = useNavigate();

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
                    className="absolute -top-[10vw] -right-[10vw] w-[50vw] max-w-[640px] rotate-6 select-none"
                    style={{opacity: 0.10}}
                    draggable={false}
                />

                {/* mid-left blob */}
                <img
                    src={blob2Url}
                    alt=""
                    aria-hidden="true"
                    className="absolute -left-[12vw] top-[40vh] w-[55vw] max-w-[720px] -rotate-6 select-none"
                    style={{opacity: 0.10}}
                    draggable={false}
                />

                {/* waves near top */}
                <img
                    src={squigglyUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[8vw] left-1/2 w-[80vw] max-w-[1000px] -translate-x-1/2 select-none"
                    style={{opacity: 0.10}}
                    draggable={false}
                />

                {/* waves near bottom */}
                <img
                    src={squigglyUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[12vw] left-1/2 w-[90vw] max-w-[1100px] -translate-x-1/2 rotate-180 select-none"
                    style={{opacity: 0.10}}
                    draggable={false}
                />

                {/* vortex */}
                <img
                    src={vortexUrl}
                    alt=""
                    aria-hidden="true"
                    className="absolute -right-[12vw] top-[70vh] w-[60vw] max-w-[720px] select-none lg:top-[64rem]"
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

                            <Body1 block>
                                Запускайте визит, следите за таймером и прогнозом стоимости в реальном времени.
                            </Body1>
                            <Body1 block>
                                Баланс, история операций и статусы профиля — всегда под рукой.
                            </Body1>

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
                                            <Subtitle2Stronger>Баланс</Subtitle2Stronger>
                                            <Badge appearance="outline">Demo</Badge>
                                        </div>
                                        <Divider className="my-3"/>
                                        <Title3>75 BYN</Title3>
                                        <Caption1>Доступно для оплаты визитов</Caption1>
                                    </HoverTiltCard>

                                    <HoverTiltCard className="h-full">
                                        <div className="flex items-center justify-between">
                                            <Subtitle2Stronger>Активный визит</Subtitle2Stronger>
                                            <Badge appearance="outline">Demo</Badge>
                                        </div>
                                        <Divider className="my-3"/>
                                        <div className="flex items-baseline justify-between">
                                            <Title3>1:42</Title3>
                                            <Caption1>примерно 15.5 BYN</Caption1>
                                        </div>
                                        <Caption1>Тариф: часовой</Caption1>
                                    </HoverTiltCard>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>

                <section className="flex flex-col gap-2">
                    <div className="flex flex-col gap-2">
                        <Title3 block>Как это работает</Title3>
                        <Body2 block>Три шага — и вы в деле</Body2>
                    </div>

                    <div className=" grid grid-cols-1 gap-3  sm:grid-cols-3">
                        {steps.map((s, idx) => (
                            <HoverTiltCard key={s.title} className="p-4 h-full">
                                <div className="flex items-start justify-between">
                                    <Subtitle2Stronger block>{s.title}</Subtitle2Stronger>
                                    <Tag appearance="brand">{idx + 1}</Tag>
                                </div>
                                <Body2>
                                    {s.description}
                                </Body2>
                            </HoverTiltCard>
                        ))}
                    </div>
                </section>

                <section className="flex flex-col gap-2">
                    <div className="flex flex-col gap-2">
                        <Title3 block>Возможности</Title3>
                        <Body2 block>Всё, что нужно гостю: визиты, баланс, профиль</Body2>
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
                                            <Subtitle2Stronger block>{f.title}</Subtitle2Stronger>
                                            <Body2>
                                                {f.description}
                                            </Body2>
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
                        <Title3 block>Тарифы</Title3>
                        <Body2 block>
                            Тариф выбирается перед визитом, оплата — по факту времени
                        </Body2>
                    </div>

                    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                        {tariffCards.map((t) => (
                            <HoverTiltCard key={t.title} className="p-4 h-full">
                                <div className="flex items-center justify-between">
                                    <Subtitle1 block>{t.title}</Subtitle1>
                                    <Tag appearance="brand">{t.highlight}</Tag>
                                </div>
                                <Body2 className="mt-2">
                                    {t.description}
                                </Body2>
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
                        <Title3 block>FAQ</Title3>
                        <Body2 block>Короткие ответы на частые вопросы</Body2>
                    </div>

                    <div>
                        <Card>
                            <Accordion collapsible defaultOpenItems={[0]}>
                                {faq.map((item, idx) => (
                                    <AccordionItem key={item.question} value={idx}>
                                        <AccordionHeader>{item.question}</AccordionHeader>
                                        <AccordionPanel>
                                            <Body2 block>{item.answer}</Body2>
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
                                <Title3 block>Готовы попробовать?</Title3>
                                <Body2 block>Создайте аккаунт и начните первый визит</Body2>
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
