// Toast notifikacije — animirane, auto-dismiss
import { useEffect, useRef } from 'react';
import { gsap } from 'gsap';

const COLORS = {
  success: { bg: 'rgba(74,222,128,0.15)', border: '#4ade8066', text: '#4ade80' },
  warning: { bg: 'rgba(248,113,113,0.15)', border: '#f8717166', text: '#f87171' },
  info: { bg: 'rgba(96,165,250,0.15)', border: '#60a5fa66', text: '#60a5fa' },
  day: { bg: 'rgba(212,160,23,0.15)', border: '#D4A01766', text: '#D4A017' },
};

const Toast = ({ notification, onRemove }) => {
  const ref = useRef(null);
  const c = COLORS[notification.type] || COLORS.info;

  useEffect(() => {
    if (ref.current) {
      gsap.fromTo(ref.current,
        { x: 60, opacity: 0 },
        { x: 0, opacity: 1, duration: 0.3, ease: 'power2.out' }
      );
    }
    const timer = setTimeout(() => {
      if (ref.current) {
        gsap.to(ref.current, {
          x: 60, opacity: 0, duration: 0.25,
          onComplete: () => onRemove(notification.id),
        });
      }
    }, 3000);
    return () => clearTimeout(timer);
  }, []);

  return (
    <div ref={ref} style={{
      background: c.bg, border: `1px solid ${c.border}`,
      borderRadius: 8, padding: '8px 14px',
      color: c.text, fontSize: 13, fontWeight: 500,
      backdropFilter: 'blur(8px)',
      whiteSpace: 'nowrap',
    }}>
      {notification.msg}
    </div>
  );
};

export const Notifications = ({ notifications, onRemove }) => (
  <div style={{
    position: 'absolute', top: 80, right: 16,
    display: 'flex', flexDirection: 'column', gap: 6,
    alignItems: 'flex-end', zIndex: 300, pointerEvents: 'none',
  }}>
    {notifications.map(n => (
      <Toast key={n.id} notification={n} onRemove={onRemove} />
    ))}
  </div>
);
