export default function PageContentContainer({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <main className="flex flex-1 flex-col gap-4 pt-0">
      <div className="container-fluid py-6">{children}</div>
    </main>
  );
}
