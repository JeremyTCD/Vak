/// <binding ProjectOpened='init' />

// note - use process.env.ASPNETCORE_ENVIRONMENT to get environment
const gulp = require('gulp');
const del = require('del');
const shell = require('gulp-shell');
// gulp 4.0 provides the function gulp.series, which makes run-sequence redundant. make sure to upgrade gulp.
const runSequence = require('run-sequence');
const browserSync = require('browser-sync').create();

gulp.task('start:browser-sync', function () {
    browserSync.init({
        proxy: "http://localhost:62875"
    });
});

gulp.task('clean', function () {
    del(['Angular/!(Aot)/*.{js,map}', 'Angular/Aot']);
});

gulp.task('build:ngc', shell.task(
    ['ngc -p tsconfig-aot.json']
));

gulp.task('build:rollup', shell.task(
    ['rollup -c rollup-config.js']
));

gulp.task('build', function () {
    runSequence('clean', 'build:ngc', 'build:rollup');
});

gulp.task('watch:aot', function () {
    gulp.watch('Angular/!(Aot)/**/*.{ts,html,css}', function (event) {
        console.log('File ' + event.path + ' was ' + event.type);
    });
    gulp.watch('Angular/!(Aot)/**/*.{ts,html,css}', ['build']);
});

gulp.task('watch:browser-sync', function () {
    gulp.watch("wwwroot/*/*.*", function (event) {
        console.log('File ' + event.path + ' was ' + event.type);
    });
    gulp.watch("wwwroot/*/*.*").on('change', browserSync.reload);
});

gulp.task('init', function () {
    runSequence('start:browser-sync', 'watch:aot', 'watch:browser-sync');
});

gulp.task('default', ['init']);
