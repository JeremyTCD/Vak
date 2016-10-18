import { JeringVectorartkitWebclientPage } from './app.po';

describe('jering-vectorartkit-webclient App', function() {
  let page: JeringVectorartkitWebclientPage;

  beforeEach(() => {
    page = new JeringVectorartkitWebclientPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
