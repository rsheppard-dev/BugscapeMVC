type ProjectSort = 'name' | 'startdate' | 'enddate' | 'pm' | 'priority'
type ProjectOrder = 'asc' | 'desc'
type Project = {
  id: number
  title: string
  developer: string
  status: string
  priority: string
  date: string
}

class ProjectsTable {
  projects: Project[]
  container: HTMLElement
  page: number
  limit: number
  sortBy: ProjectSort
  order: ProjectOrder

  constructor(
    projects: Project[],
    container?: HTMLElement,
    page?: number,
    limit?: number,
    sortBy?: ProjectSort,
    order?: ProjectOrder
  ) {
    this.projects = projects ?? [],
    this.container = container ?? document.querySelector('[data-container="projects"]') as HTMLElement,
    this.page = page ?? 1,
    this.limit = limit ?? 4,
    this.sortBy = sortBy ?? 'name',
    this.order = order ?? 'asc'
  }

  async init() {
    this.getProjects()

    this.container.addEventListener('click', async (event) => {
      const header = (event.target as HTMLElement).closest('[data-sortable]')
      if (header) {
        if (header.getAttribute('data-order') === this.order)
          this.order =
            header.getAttribute('data-order') === 'asc' ? 'desc' : 'asc'

        this.page =
          parseInt(header.getAttribute('data-page') as string) ?? this.page
        this.limit =
          parseInt(header.getAttribute('data-limit') as string) ?? this.limit
        this.sortBy = (header.getAttribute('data-sort') as ProjectSort) ?? this.sortBy

        this.getProjects()
      }
    })
  }

  async getProjects() {
    try {
      const url = `/Projects/SortProjects?page=${this.page}&sortBy=${this.sortBy}&order=${this.order}&limit=${this.limit}`

      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(this.projects),
      })

      if (!response.ok) {
        throw new Error(`Error loading projects table: ${response.statusText}`)
      }

      const html = await response.text()

      this.container.innerHTML = html

      const header = this.container.querySelector(
        `[data-sort="${this.sortBy}"]`
      )!
      header.setAttribute('data-order', this.order as ProjectOrder)

      header.classList.add('active')

      const arrow = header.querySelector('.order')

      arrow?.classList.add(this.order as ProjectOrder)
    } catch (error) {
      console.error(error)
    }
  }
}
