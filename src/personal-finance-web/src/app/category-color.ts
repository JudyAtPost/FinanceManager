/** A small, pleasant palette used to consistently color-code categories across charts and lists. */
const PALETTE = [
  '#2563eb', // blue
  '#16a34a', // green
  '#d97706', // amber
  '#dc2626', // red
  '#7c3aed', // violet
  '#0891b2', // cyan
  '#db2777', // pink
  '#65a30d' // lime
];

/**
 * Deterministically maps a category name to a color from {@link PALETTE}, so the same
 * category always renders with the same color across the donut chart, budgets, and lists.
 */
export function categoryColor(categoryName: string): string {
  let hash = 0;
  for (let i = 0; i < categoryName.length; i++) {
    hash = (hash << 5) - hash + categoryName.charCodeAt(i);
    hash |= 0;
  }

  const index = Math.abs(hash) % PALETTE.length;
  return PALETTE[index];
}
