"use client";

import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/shared/components/ui/pagination";

type LocationsPaginationProps = {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

function getPaginationPages(currentPage: number, totalPages: number) {
  const pages: Array<number | "..."> = [];

  if (totalPages <= 7) {
    for (let page = 1; page <= totalPages; page += 1) {
      pages.push(page);
    }

    return pages;
  }

  pages.push(1);

  if (currentPage > 3) {
    pages.push("...");
  }

  const startPage = Math.max(2, currentPage - 1);
  const endPage = Math.min(totalPages - 1, currentPage + 1);

  for (let page = startPage; page <= endPage; page += 1) {
    pages.push(page);
  }

  if (currentPage < totalPages - 2) {
    pages.push("...");
  }

  pages.push(totalPages);

  return pages;
}

export function LocationsPagination({
  currentPage,
  totalPages,
  onPageChange,
}: LocationsPaginationProps) {
  const paginationPages = getPaginationPages(currentPage, totalPages);

  function handlePageChange(nextPage: number) {
    if (nextPage < 1 || nextPage > totalPages || nextPage === currentPage) {
      return;
    }

    onPageChange(nextPage);
  }

  return (
    <section className="rounded-2xl border border-[#2f281f] bg-[#111816] p-4">
      <div className="mb-4 text-center text-base text-stone-400">
        Страница {currentPage} из {totalPages}
        <span className="ml-2 text-stone-500"> · по 2 на странице</span>
      </div>

      <Pagination>
        <PaginationContent>
          <PaginationItem>
            <PaginationPrevious
              href="#"
              text="Назад"
              onClick={(event) => {
                event.preventDefault();
                handlePageChange(currentPage - 1);
              }}
              className={
                currentPage <= 1
                  ? "pointer-events-none opacity-40"
                  : "text-stone-200"
              }
            />
          </PaginationItem>

          {paginationPages.map((paginationPage, index) =>
            paginationPage === "..." ? (
              <PaginationItem key={`ellipsis-${index}`}>
                <PaginationEllipsis className="text-stone-500" />
              </PaginationItem>
            ) : (
              <PaginationItem key={paginationPage}>
                <PaginationLink
                  href="#"
                  isActive={paginationPage === currentPage}
                  onClick={(event) => {
                    event.preventDefault();
                    handlePageChange(paginationPage);
                  }}
                  className={
                    paginationPage === currentPage
                      ? "border-emerald-900/70 bg-emerald-950/40 text-emerald-300"
                      : "text-stone-300 hover:text-white"
                  }
                >
                  {paginationPage}
                </PaginationLink>
              </PaginationItem>
            )
          )}

          <PaginationItem>
            <PaginationNext
              href="#"
              text="Вперёд"
              onClick={(event) => {
                event.preventDefault();
                handlePageChange(currentPage + 1);
              }}
              className={
                currentPage >= totalPages
                  ? "pointer-events-none opacity-40"
                  : "text-stone-200"
              }
            />
          </PaginationItem>
        </PaginationContent>
      </Pagination>
    </section>
  );
}