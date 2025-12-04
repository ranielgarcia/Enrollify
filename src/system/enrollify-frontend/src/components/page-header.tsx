export default function PageHeader({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <header className="bg-background sticky top-0 z-10 border-b">
      <div className="container-fluid flex h-16 items-center justify-between py-4">
        {children}
      </div>
    </header>
  );
}
