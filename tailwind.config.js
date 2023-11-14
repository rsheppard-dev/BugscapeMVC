/** @type {import('tailwindcss').Config} */
const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
  content: ['**/*.razor', '**/*.cshtml', '**/*.html'],
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
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
    require('tailwind-scrollbar')({ nocompatible: true }),
  ],
}
