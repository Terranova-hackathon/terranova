export const metadata = {
  title: process.env.PROJECT_NAME,
  description: 'Im not sure what we are building yet',
};

export default function RootLayout({ children }) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
