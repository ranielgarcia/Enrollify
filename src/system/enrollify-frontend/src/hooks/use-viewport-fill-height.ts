import { useLayoutEffect, useRef, useState } from "react";

/**
 * Calculates the remaining viewport height from an element's top position
 * to the bottom of the viewport, allowing elements like data tables to
 * fill the available space while the rest of the page scrolls naturally.
 */
export function useViewportFillHeight<T extends HTMLElement>(
  bottomOffset = 0,
  minHeight = 200,
) {
  const ref = useRef<T>(null);
  const [height, setHeight] = useState<number | undefined>(undefined);

  useLayoutEffect(() => {
    const element = ref.current;
    if (!element) return;

    const updateHeight = () => {
      const top = element.getBoundingClientRect().top;
      const available = window.innerHeight - top - bottomOffset;
      const newHeight = Math.max(minHeight, available);
      setHeight((prev) =>
        prev !== undefined && Math.abs(prev - newHeight) < 1 ? prev : newHeight,
      );
    };

    updateHeight();

    const observer = new ResizeObserver(updateHeight);
    // Observe ancestor chain to catch layout shifts (e.g. sidebar collapse)
    let ancestor: HTMLElement | null = element;
    while (ancestor) {
      observer.observe(ancestor);
      ancestor = ancestor.parentElement;
    }

    window.addEventListener("resize", updateHeight);

    return () => {
      observer.disconnect();
      window.removeEventListener("resize", updateHeight);
    };
  }, [bottomOffset, minHeight]);

  return { ref, height };
}
