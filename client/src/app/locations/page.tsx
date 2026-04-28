export default function LocationsPage() {
  return (
    <section className="rounded-3xl border border-[#2f281f] bg-[#111816] p-6 sm:p-8 lg:p-10">
      <p className="text-xs uppercase tracking-[0.2em] text-emerald-300">
        раздел локаций
      </p>

      <h1 className="mt-3 text-3xl font-semibold text-stone-50 sm:text-4xl">
        Локации
      </h1>

      <p className="mt-4 max-w-3xl text-sm leading-7 text-stone-300 sm:text-base">
        В этом разделе собрана информация о рабочих локациях компании: офисах,
        зданиях, филиалах и других площадках.
      </p>

      <p className="mt-4 max-w-3xl text-sm leading-7 text-stone-400 sm:text-base">
        Здесь можно быстро посмотреть, какие объекты есть в компании и где
        именно они расположены.
      </p>
    </section>
  );
}