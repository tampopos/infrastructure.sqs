const gulp = require('gulp');
const { clean, restore, build, test } = require('gulp-dotnet-cli');
const lib = ['**/Tmpps.Infrastructure.SQS.csproj'];
const tests = '**/*.Tests.csproj';

//clean
gulp.task('clean', () => {
  return gulp.src(tests, { read: false }).pipe(clean());
});
//restore nuget packages
gulp.task('restore', () => {
  return gulp.src(tests, { read: false }).pipe(restore());
});
//compile
gulp.task('build', () => {
  return gulp.src(tests, { read: false }).pipe(build());
});
gulp.task('test', () => {
  return gulp.src(tests, { read: false }).pipe(test());
});
gulp.task('publish-inner', () => {
  return gulp
    .src(lib, { read: false })
    .pipe(build({ configuration: 'Release' }));
});
gulp.task('publish', gulp.series('test', 'publish-inner'));
