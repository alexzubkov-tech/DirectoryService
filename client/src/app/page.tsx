const sections = [
  {
    title: "Подразделения",
    description:
      "Просматривайте структуру компании: отделы, направления, филиалы и связи между ними.",
  },
  {
    title: "Локации",
    description:
      "Узнавайте, где расположены офисы, здания и рабочие площадки компании.",
  },
  {
    title: "Должности",
    description:
      "Смотрите перечень должностей и ориентируйтесь, в каких подразделениях они используются.",
  },
];

const features = [
  "быстро находить нужное подразделение",
  "понимать, где расположены отделы и офисы",
  "смотреть, какие должности есть в компании",
  "удобно ориентироваться в структуре организации",
];

export default function Home() {
  return (
    <div className="flex w-full flex-col gap-6">
      <section className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8 lg:p-10">
        <span className="inline-flex rounded-full border border-emerald-900/60 bg-emerald-950/40 px-3 py-1 text-xs uppercase tracking-[0.2em] text-emerald-300">
          корпоративный справочник
        </span>

        <h1 className="mt-4 max-w-4xl text-3xl font-semibold tracking-tight text-stone-50 sm:text-4xl lg:text-5xl">
          Справочник компании
        </h1>

        <p className="mt-4 max-w-4xl text-sm leading-7 text-stone-300 sm:text-base">
          Это приложение помогает быстро ориентироваться в структуре компании.
          Здесь можно посмотреть подразделения, рабочие локации и перечень
          должностей, чтобы легче находить нужную информацию и понимать, как
          устроена организация.
        </p>

        <p className="mt-4 max-w-4xl text-sm leading-7 text-stone-400 sm:text-base">
          Сервис собран в одном месте для удобной навигации по основным
          справочным разделам компании и предназначен для повседневного
          использования сотрудниками.
        </p>
      </section>

      <section className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
        {sections.map((item) => (
          <article
            key={item.title}
            className="rounded-2xl border border-[#2f281f] bg-[#111816] p-5"
          >
            <h2 className="text-lg font-semibold text-stone-100">
              {item.title}
            </h2>
            <p className="mt-3 text-sm leading-7 text-stone-300">
              {item.description}
            </p>
          </article>
        ))}
      </section>

      <section className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8">
        <h2 className="text-2xl font-semibold text-stone-100">
          Что можно сделать в приложении
        </h2>

        <div className="mt-5 grid grid-cols-1 gap-3 md:grid-cols-2">
          {features.map((item) => (
            <div
              key={item}
              className="rounded-2xl border border-[#2f281f] bg-[#0d1210] px-4 py-3 text-sm text-stone-300"
            >
              {item}
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}