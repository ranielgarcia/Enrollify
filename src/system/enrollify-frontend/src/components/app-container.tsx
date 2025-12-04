export default function Container({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return <div className="px-4 lg:px-6">{children}</div>;
}
