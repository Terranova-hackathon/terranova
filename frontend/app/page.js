export default function HomePage() {
  return (
    <main style={{ fontFamily: 'Arial, sans-serif', padding: '3rem', maxWidth: '900px', margin: '0 auto' }}>
      <h1 style={{ fontSize: '3rem', marginBottom: '1rem' }}>{process.env.PROJECT_NAME}</h1>
      <p style={{ fontSize: '1.1rem', lineHeight: 1.7, color: '#12fbffff' }}>
        {process.env.PROJECT_NAME + " " }
        <>
          lets add some text here.
        </>
      </p>
    </main>
  );
}
