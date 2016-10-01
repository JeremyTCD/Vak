import { platformBrowser } from '@angular/platform-browser';
import { AppModuleNgFactory } from '../Aot/Angular/SignUpApp/sign-up-app.module.ngfactory';

platformBrowser().bootstrapModuleFactory(AppModuleNgFactory);
