import {
    LargeTitle,
    Title1,
    Title2,
    Title3,
    Subtitle2Stronger,
    Body1,
    Body2,
    Caption1,
    Button,
    Card,
    Input,
    ProgressBar,
    tokens,
    Text, Display, Divider,
    Caption1Strong, type TableColumnDefinition,
} from "@fluentui/react-components";

import {
    Money20Regular,
    Add20Regular,
    Warning20Filled,
    Gift20Regular,
    ChatRegular,
    CallRegular
} from "@fluentui/react-icons";

import vortex from "../../assets/vvvortex.svg";
import surf from "../../assets/sssurf.svg";
import repeat from "../../assets/rrrepeat (2).svg";
import {useMemo} from "react";
import {ResponsiveContainer, VerticalBarChart, type VerticalBarChartDataPoint} from "@fluentui/react-charts";

import {
    createTableColumn,
    Avatar
} from "@fluentui/react-components";
import {DataTable} from "../../components/DataTable/DataTable";


interface Transaction {
    id: string;
    icon: string | React.ReactElement;
    title: string;
    sub: string;
    amount: number;
    status: string;
    isPositive?: boolean;
}

export const BalancePage = () => {
    const chartPoints: VerticalBarChartDataPoint[] = useMemo(() => [
        {x: 1, y: 1500, color: tokens.colorBrandBackground, legend: 'Пополнения'},
        {x: 2, y: 2200, color: tokens.colorBrandBackground, legend: 'Пополнения'},
        {x: 3, y: 800, color: tokens.colorBrandBackground, legend: 'Пополнения'},
        {x: 3, y: -600, color: tokens.colorStatusDangerBackground3, legend: 'Списание'},
        {x: 4, y: 3100, color: tokens.colorBrandBackground, legend: 'Пополнения'},
        {x: 5, y: 1900, color: tokens.colorBrandBackground, legend: 'Пополнения'},
        {x: 6, y: 4500, color: tokens.colorBrandBackground, legend: 'Пополнения'},
        {x: 7, y: 2800, color: tokens.colorBrandBackground, legend: 'Пополнения'},
    ], []);


    const columns: TableColumnDefinition<Transaction>[] = [
        createTableColumn<Transaction>({
            columnId: "info",
            renderHeaderCell: () => "Операция",
            renderCell: (item) => (
                <div className="flex items-center gap-3">
                    <Avatar
                        initials={typeof item.icon === 'string' ? item.icon : undefined}
                        icon={typeof item.icon !== 'string' ? item.icon : undefined}
                        aria-label={item.title}
                        color="neutral"
                        size={32}
                        shape="circular"
                    />
                    <div className="flex flex-col">
                        <Subtitle2Stronger block>{item.title}</Subtitle2Stronger>
                        <Caption1 block style={{color: tokens.colorNeutralForeground3}}>{item.sub}</Caption1>
                    </div>
                </div>
            ),
        }),
        createTableColumn<Transaction>({
            columnId: "amount",
            renderHeaderCell: () => "Сумма",
            renderCell: (item) => (
                <div>
                    <Subtitle2Stronger block style={{
                        color: item.isPositive ? tokens.colorPaletteGreenForeground1 : tokens.colorPaletteRedForeground1
                    }}>
                        {item.isPositive ? "+" : ""}{item.amount.toLocaleString()} ₽
                    </Subtitle2Stronger>
                    <Caption1>{item.status}</Caption1>
                </div>
            ),
        }),
    ];

    const transactions: Transaction[] = [
        {
            id: "1",
            icon: "☕",
            title: "Визит: Главный зал",
            sub: "Сегодня, 14:20 • 2ч 15мин",
            amount: 525,
            status: "Завершено"
        },
        {
            id: "2",
            icon: <Add20Regular/>,
            title: "Пополнение баланса",
            sub: "Вчера, 18:10 • Stripe",
            amount: 2000,
            status: "Успешно",
            isPositive: true
        }
    ];

    return (
        <div className="relative tc-noise-overlay w-full h-full overflow-hidden">
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
                <img
                    src={vortex}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[10vw] -left-[10vw] w-[50vw] max-w-[640px] select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={repeat}
                    alt=""
                    aria-hidden="true"
                    className="absolute -top-[6vw] -right-[8vw] w-[40vw] max-w-[520px] select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] left-0 w-[100vw] max-w-none select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
                <img
                    src={surf}
                    alt=""
                    aria-hidden="true"
                    className="absolute -bottom-[16vw] right-0 -scale-x-100 w-[100vw] max-w-none select-none"
                    style={{opacity: 0.30}}
                    draggable={false}
                />
            </div>

            <div className="relative mx-auto w-full max-w-6xl px-2 py-4 sm:px-3 sm:py-6">
                <div className="flex flex-col gap-4 overflow-hidden rounded-3xl p-5 sm:p-8"
                     style={{
                         backgroundImage: `radial-gradient(900px 480px at 20% 10%, ${tokens.colorBrandBackground2} 0%, transparent 60%), radial-gradient(720px 420px at 90% 0%, ${tokens.colorPaletteLightGreenBackground2} 0%, transparent 55%), linear-gradient(180deg, ${tokens.colorNeutralBackground1} 0%, ${tokens.colorNeutralBackground2} 100%)`,
                         border: `1px solid ${tokens.colorNeutralStroke1}`,
                         boxShadow: tokens.shadow16,
                     }}>
                    {/* Header */}
                    <div className="mb-6 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
                        <LargeTitle>Управление балансом</LargeTitle>
                    </div>

                    <div className="flex flex-col gap-4">

                        {/* Главная карта баланса */}
                        <Card className="flex !flex-col gap-4 sm:!flex-row">
                            <div className="flex flex-col gap-4 ">
                                <Body1 block
                                       style={{
                                           textTransform: 'uppercase'
                                       }}>
                                    Доступно для оплаты
                                </Body1>
                                <div className="flex flex-wrap items-baseline gap-3">
                                    <Display truncate wrap={false}>3500 ₽</Display>
                                    <Text weight="semibold"
                                          style={{color: tokens.colorPaletteGreenForeground1}}>↑
                                        12% за месяц</Text>
                                </div>
                            </div>

                            {/* Мини-график на CSS */}
                            <div className="flex flex-col gap-4 w-full">
                                <Subtitle2Stronger>Активность</Subtitle2Stronger>

                                <ResponsiveContainer height={200}>
                                    <VerticalBarChart
                                        data={chartPoints}
                                        maxBarWidth={250}
                                        hideLegend={true}
                                        barWidth={"auto"}
                                        roundCorners={true}
                                        hideLabels={true}
                                        roundedTicks={true}
                                        enableGradient={true}
                                        onRenderCalloutPerDataPoint={(props) => {
                                            if (!props) return null;
                                            return (
                                                <div className="flex items-stretch gap-3">
                                                    <div
                                                        className="w-1 rounded-full"
                                                        style={{backgroundColor: props.color}}
                                                    />

                                                    <div className="flex flex-col justify-center">
                                                        <Caption1Strong block>
                                                            {props.legend}: {props.x} число
                                                        </Caption1Strong>
                                                        <Title3
                                                            style={{color: props.color}}>
                                                            {props.y} ₽
                                                        </Title3>
                                                    </div>
                                                </div>
                                            );
                                        }}
                                    />
                                </ResponsiveContainer>

                                <Divider/>

                                <div className="flex flex-wrap justify-between items-center">
                                    <Caption1>
                                        Всего за неделю
                                    </Caption1>
                                    <Text weight="semibold">16 800 ₽</Text>
                                </div>
                            </div>
                        </Card>

                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            {/* Быстрое пополнение */}
                            <Card className="flex flex-col gap-4">
                                <Title3 block>Быстрое пополнение</Title3>
                                <Input
                                    type="number"
                                    placeholder="Введите сумму (₽)"
                                    contentBefore={<Money20Regular/>}
                                    className="w-full"
                                />
                                <div className="flex flex-wrap gap-4 ">
                                    {['+500', '+1000', '+2500', '+5000'].map(val => (
                                        <Button key={val} appearance="outline">{val}</Button>
                                    ))}
                                </div>
                                <Button appearance="primary" icon={<Add20Regular/>}>Перейти к оплате
                                    Stripe</Button>
                            </Card>

                            {/* Способ оплаты */}
                            <Card className="flex flex-col gap-4 p-6">
                                <Title3 block>Способ оплаты</Title3>
                                <Card appearance="outline" className="flex flex-row items-center justify-between p-3"
                                      style={{borderColor: tokens.colorBrandStroke1}}>
                                    <div className="flex items-center gap-3">
                                        <div
                                            className="flex h-6 w-10 items-center justify-center rounded bg-black text-[8px] font-black text-white">VISA
                                        </div>
                                        <div>
                                            <Subtitle2Stronger block>•••• 4421</Subtitle2Stronger>
                                            <Caption1 style={{color: tokens.colorNeutralForeground3}}>Истекает
                                                12/28</Caption1>
                                        </div>
                                    </div>
                                    <Button appearance="subtle" size="small">ИЗМЕНИТЬ</Button>
                                </Card>
                                <div className="flex items-center justify-between mt-2">
                                    <Body2>Автоплатеж</Body2>
                                    <div className="h-5 w-10 rounded-full relative"
                                         style={{backgroundColor: tokens.colorBrandBackground}}>
                                        <div className="absolute right-1 top-1 h-3 w-3 rounded-full bg-white"/>
                                    </div>
                                </div>
                            </Card>
                        </div>

                        {/* История операций */}
                        <Card className="flex flex-col gap-4 h-full">
                            <div className="flex items-center justify-between">
                                <Title1>История операций</Title1>
                                <Button appearance="subtle" size="small">Фильтры</Button>
                            </div>

                            {/* Используем твой DataTable вместо .map() */}
                            <div className="overflow-hidden">
                                <DataTable
                                    items={transactions} // Данные из стейта/редюсера
                                    columns={columns}
                                    getRowId={(item) => item.id}
                                />
                            </div>

                            <Button appearance="outline">
                                Загрузить все транзакции
                            </Button>
                        </Card>
                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            {/* Время отдыха */}
                            <Card className="h-full border-none" style={{
                                backgroundColor: tokens.colorBrandBackground,
                                color: tokens.colorNeutralBackground1
                            }}>
                                <Caption1 block style={{opacity: 0.8}}>Хватит на отдых</Caption1>
                                <Title2 block style={{fontSize: '32px', color: 'white'}}>~ 8ч 15мин</Title2>

                                <div className="mt-auto">
                                    <Caption1 block style={{opacity: 0.7}}>Расчёт по тарифу:</Caption1>
                                    <Subtitle2Stronger block>Стандарт (7 ₽/мин)</Subtitle2Stronger>
                                </div>
                            </Card>

                            {/* Warning: Задолженность */}
                            <Card appearance="subtle"
                                  className="border-2 border-dashed flex flex-col sm:flex-row items-center justify-between gap-4"
                                  style={{
                                      borderColor: tokens.colorPaletteRedBorder2,
                                      backgroundColor: tokens.colorPaletteRedBackground1
                                  }}>
                                <div className="flex items-center gap-4">
                                    <div className="flex h-10 w-10 items-center justify-center rounded-full shrink-0"
                                         style={{backgroundColor: tokens.colorPaletteRedBackground2}}>
                                        <Warning20Filled style={{color: tokens.colorPaletteRedForeground1}}/>
                                    </div>
                                    <div>
                                        <Title3 block style={{color: tokens.colorPaletteRedForeground1}}>Внимание:
                                            задолженность</Title3>
                                        <Body1 style={{color: tokens.colorNeutralForeground2}}>Ваш баланс ниже нуля.
                                            Пожалуйста, погасите долг 200.00 ₽</Body1>
                                    </div>
                                </div>
                                <Button appearance="primary"
                                        style={{backgroundColor: tokens.colorPaletteRedBackground3}}>Погасить
                                    сейчас</Button>
                            </Card>
                        </div>


                        {/* Лояльность */}
                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                            <Card className="flex flex-col gap-4 ">
                                <Title3>Лояльность</Title3>
                                <div className="text-center">
                                    <Caption1 block style={{color: tokens.colorNeutralForeground3}}>Ваш
                                        статус</Caption1>
                                    <Title2 style={{color: tokens.colorPaletteBeigeForeground2}}>СЕРЕБРО</Title2>
                                </div>
                                <ProgressBar value={0.75} color="brand"/>
                                <div className="flex justify-between">
                                    <Caption1>750 ₽ потрачено</Caption1>
                                    <Caption1>250 ₽ до ЗОЛОТА</Caption1>
                                </div>
                                <Card appearance="subtle" style={{backgroundColor: tokens.colorBrandBackground2}}>
                                    <div className="flex gap-2">
                                        <Gift20Regular/>
                                        <Caption1>Ваша скидка 5% уже применяется ко всем визитам.</Caption1>
                                    </div>
                                </Card>
                            </Card>

                            {/* Инфо по тарифу */}
                            <Card>
                                <Caption1 block
                                          style={{color: tokens.colorNeutralForeground3, textTransform: 'uppercase'}}>Информация
                                    о тарифе</Caption1>
                                <Title2 block>Стандартный</Title2>
                                <div className="flex flex-col gap-3">
                                    <div className="flex justify-between border-b border-neutral-100 pb-2">
                                        <Body1>Первый час</Body1>
                                        <Subtitle2Stronger>250 ₽</Subtitle2Stronger>
                                    </div>
                                    <div className="flex justify-between border-b border-neutral-100 pb-2">
                                        <Body1>Последующие минуты</Body1>
                                        <Subtitle2Stronger>5 ₽ / мин</Subtitle2Stronger>
                                    </div>
                                    <div className="flex justify-between">
                                        <Body1>Стоп-чек (весь день)</Body1>
                                        <Subtitle2Stronger>1 200 ₽</Subtitle2Stronger>
                                    </div>
                                </div>
                            </Card>
                        </div>


                        {/* Заметки */}
                        <Card className="flex h-full flex-col p-6">
                            <div className="flex items-center gap-2 mb-2">
                                <ChatRegular style={{color: tokens.colorBrandForeground1}}/>
                                <Caption1 block style={{
                                    color: tokens.colorNeutralForeground3,
                                    textTransform: 'uppercase'
                                }}>
                                    Поддержка
                                </Caption1>
                            </div>

                            <Subtitle2Stronger block>Нужна помощь?</Subtitle2Stronger>
                            <Body2 block>
                                Администратор на связи и готов ответить на ваши вопросы по работе антикафе.
                            </Body2>

                            <div className="mt-auto flex flex-wrap gap-2">
                                <Button
                                    appearance="primary"
                                    icon={<ChatRegular/>}
                                    onClick={() => window.open('https://t.me/your_admin', '_blank')}
                                >
                                    Чат в Telegram
                                </Button>
                                <Button
                                    appearance="secondary"
                                    icon={<CallRegular/>}
                                >
                                    Позвать админа
                                </Button>
                            </div>
                        </Card>
                    </div>
                </div>
            </div>
        </div>
    );
};