import { FileBlob, PresentationFile } from "@oai/artifact-tool";

const PPTX = "E:/UnityLearn/Kitchen/output/doc/基于Unity的多人协作烹饪游戏设计与实现_答辩PPT_内容丰富版.pptx";

const blob = await FileBlob.load(PPTX);
const presentation = await PresentationFile.importPptx(blob);
console.log(`slides=${presentation.slides.count}`);
console.log(PPTX);
