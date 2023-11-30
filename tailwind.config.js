/** @type {import('tailwindcss').Config} */
const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
  content: ['**/*.razor', '**/*.cs', '**/*.cshtml', '**/*.html'],
  theme: {
    container: {
      center: true,
    },
    extend: {
      colors: {
        danger: '#EF476F',
        info: '#00B4D8',
        warning: '#FFD166',
        success: '#0AD69F',
        dark: '#111111',
        light: '#C0D0CD',
        mid: '#EDF2EF',
        lightest: '#F8F9FA',
      },
      fontFamily: {
        rockwell: ['rockwell', ...defaultTheme.fontFamily.sans],
        lato: ['lato', ...defaultTheme.fontFamily.sans],
      },
      keyframes: {
        'fade-in': {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        'fade-out': {
          '0%': { opacity: '1' },
          '100%': { opacity: '0' },
        },
      },
      animation: {
        'fade-in': 'fade-in 0.5s ease-in-out',
        'fade-out': 'fade-out 0.5s ease-in-out',
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
    require('tailwind-scrollbar')({ nocompatible: true }),
  ],
}
