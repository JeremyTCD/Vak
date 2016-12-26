import { AppSegments } from './app.segments';

export class AppPaths {
    static errorPath: string = '/' + AppSegments.errorSegment;

    static homePath: string = '/' + AppSegments.homeSegment;

    static logInPath: string = '/' + AppSegments.logInSegment;
    static forgotPasswordPath: string = `/${AppSegments.logInSegment}/${AppSegments.forgotPasswordSegment}`;
    static resetPasswordPath: string = `/${AppSegments.logInSegment}/${AppSegments.resetPasswordSegment}`;
    static twoFactorAuthPath: string = `/${AppSegments.logInSegment}/${AppSegments.twoFactorAuthSegment}`;

    static manageAccountPath: string = '/' + AppSegments.manageAccountSegment;
    static changeAltEmailPath: string = `/${AppSegments.manageAccountSegment}/${AppSegments.changeAltEmailSegment}`;
    static changeDisplayNamePath: string = `/${AppSegments.manageAccountSegment}/${AppSegments.changeDisplayNameSegment}`;
    static changeEmailPath: string = `/${AppSegments.manageAccountSegment}/${AppSegments.changeEmailSegment}`;
    static changePasswordPath: string = `/${AppSegments.manageAccountSegment}/${AppSegments.changePasswordSegment}`;
    static twoFactorVerifyEmailPath: string = `/${AppSegments.manageAccountSegment}/${AppSegments.twoFactorVerifyEmailSegment}`;
    static verifyAltEmailPath: string = `/${AppSegments.manageAccountSegment}/${AppSegments.verifyAltEmailSegment}`;
    static verifyEmailPath: string = `/${AppSegments.manageAccountSegment}/${AppSegments.verifyEmailSegment}`;

    static signUpPath: string = '/' + AppSegments.signUpSegment;
}
