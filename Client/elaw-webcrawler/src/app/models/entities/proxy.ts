import { PageUrl } from "./pageUrl";

export type Proxy = {
    id: string;
    startDate: string;
    endDate: string;
    page: number;
    row: number;
    requestKey: string;
    jsonFileAddress: string;
    pagesUrl: PageUrl[]
};