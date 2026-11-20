import { Fragment, type ReactNode } from "react";
import { cn } from "@/lib/utils";

/**
 * Opis kolumny tabeli. `align: "right"` dla kwot i akcji. `hideBelow` chowa kolumnę na wąskich ekranach.
 * `srOnlyHeader` zostawia nagłówek tylko dla czytnika ekranu (np. kolumna z przyciskami).
 */
export interface NautivoColumn<T> {
  key: string;
  header: ReactNode;
  cell: (row: T) => ReactNode;
  align?: "left" | "right";
  hideBelow?: "sm" | "md";
  srOnlyHeader?: boolean;
  className?: string;
}

const HIDE = { sm: "hidden sm:table-cell", md: "hidden md:table-cell" } as const;

/**
 * Jedyna tabela danych w aplikacji: karta, nagłówek, wiersze z separatorami i opcjonalna stopka.
 * `extraRows` dokleja własne wiersze na końcu (np. formularz dodawania), `footer` to wiersze <tr> w <tfoot>.
 * `renderRow` może zastąpić cały wiersz własnym <tr> (np. edycja w miejscu); zwrócenie undefined zostawia zwykły wiersz.
 */
export function NautivoTable<T>({
  columns,
  rows,
  rowKey,
  caption,
  minWidth = 640,
  extraRows,
  footer,
  rowClassName,
  renderRow,
}: {
  columns: NautivoColumn<T>[];
  rows: T[];
  rowKey: (row: T) => string;
  caption?: string;
  minWidth?: number;
  extraRows?: ReactNode;
  footer?: ReactNode;
  rowClassName?: (row: T) => string | undefined;
  renderRow?: (row: T) => ReactNode | undefined;
}) {
  const cellClass = (c: NautivoColumn<T>) => cn("p-3", c.align === "right" && "text-right", c.hideBelow && HIDE[c.hideBelow], c.className);
  return (
    <div className="card overflow-x-auto">
      <table className="w-full text-left text-sm" style={{ minWidth }}>
        {caption && <caption className="sr-only">{caption}</caption>}
        <thead className="border-b text-muted-foreground">
          <tr>
            {columns.map((c) => (
              <th key={c.key} scope="col" className={cn(cellClass(c), "font-medium")}>
                {c.srOnlyHeader ? <span className="sr-only">{c.header}</span> : c.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y">
          {rows.map((row) => {
            const custom = renderRow?.(row);
            if (custom !== undefined) return <Fragment key={rowKey(row)}>{custom}</Fragment>;
            return (
              <tr key={rowKey(row)} className={rowClassName?.(row)}>
                {columns.map((c) => (
                  <td key={c.key} className={cellClass(c)}>
                    {c.cell(row)}
                  </td>
                ))}
              </tr>
            );
          })}
          {extraRows}
        </tbody>
        {footer && <tfoot className="border-t font-semibold">{footer}</tfoot>}
      </table>
    </div>
  );
}

/** Rząd przycisków akcji w ostatniej kolumnie tabeli. */
export function NautivoRowActions({ children }: { children: ReactNode }) {
  return <div className="-my-1.5 flex items-center justify-end gap-1">{children}</div>;
}
