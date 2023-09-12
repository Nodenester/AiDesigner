/** @type {import('tailwindcss').Config} */
module.exports = {
    darkMode: 'class',
    content: ['./**/*.razor'], 
    theme: {
        extend: {
            colors: {
                gray: {
                    500: '#2f4f4f',
                    600: '#353535',
                    700: '#2b2b2b',
                    800: '#2b2b2b',
                },
            },
        },
    },
    variants: {},
    plugins: [],
}

